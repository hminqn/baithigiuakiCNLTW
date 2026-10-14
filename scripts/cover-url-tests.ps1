param([string]$BaseUrl = "http://localhost:5081")

$ErrorActionPreference = "Stop"
$BaseUrl = $BaseUrl.TrimEnd('/')
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$passed = 0
$dishIds = [System.Collections.Generic.List[string]]::new()
$categoryId = $null

function Check($condition, [string]$message) {
    if (-not $condition) { throw "FAIL: $message" }
    $script:passed++
    Write-Host "PASS: $message"
}
function Get-Page([string]$path) {
    $response = Invoke-WebRequest "$BaseUrl$path" -WebSession $session -SkipHttpErrorCheck
    [pscustomobject]@{ StatusCode = $response.StatusCode; Content = [System.Net.WebUtility]::HtmlDecode($response.Content) }
}
function Send-Form([string]$path, [hashtable]$body, [string]$tokenPage) {
    $page = Get-Page $tokenPage
    $match = [regex]::Match($page.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
    if (-not $match.Success) { throw "Antiforgery token missing: $tokenPage" }
    $body["__RequestVerificationToken"] = $match.Groups[1].Value
    $response = Invoke-WebRequest "$BaseUrl$path" -Method Post -Body $body -WebSession $session -SkipHttpErrorCheck
    [pscustomobject]@{ StatusCode = $response.StatusCode; Content = [System.Net.WebUtility]::HtmlDecode($response.Content) }
}
function Input-Value([string]$content, [string]$name) {
    $escapedName = [regex]::Escape($name)
    $inputTag = [regex]::Match($content, '<input\b(?=[^>]*\bname="' + $escapedName + '")[^>]*>')
    if (-not $inputTag.Success) { throw "Input missing: $name" }
    [regex]::Match($inputTag.Value, '\bvalue="([^"]*)"').Groups[1].Value
}
function Image-Ids([string]$content) {
    @([regex]::Matches($content, 'action="/Dishes/DeleteImage"[^>]*>\s*<input name="id" type="hidden" value="(\d+)"') | ForEach-Object { $_.Groups[1].Value })
}
function Image-Count([string]$content) { ([regex]::Matches($content, '<article class="image-card">')).Count }
function Cover-Count([string]$content) { ([regex]::Matches($content, 'time-badge quick">(?:Cover image|Ảnh đại diện)')).Count }
function Dish-Body([string]$id, [string]$name, [string]$url) {
    @{
        Id = $id; Name = $name; Price = "12345"; PreparationTime = "15";
        DishCategoryId = $script:categoryId; IsAvailable = "true";
        Description = "Temporary cover URL integration test"; CoverImageUrl = $url
    }
}
function Track-Dish($response) {
    $id = [regex]::Match($response.Content, '/Dishes/Edit/(\d+)').Groups[1].Value
    if (-not [string]::IsNullOrEmpty($id)) { $script:dishIds.Add($id) }
    $id
}

# Requests go only to the application. example.test URLs exercise storage and
# rendering; Invoke-WebRequest does not request the images referenced in HTML.
$suffix = [Guid]::NewGuid().ToString("N").Substring(0, 12)
$categoryName = "CoverUrlCategory_$suffix"
$coverDishName = "CoverUrlDish_$suffix"
$emptyDishName = "CoverUrlEmpty_$suffix"
$httpsUrl = "https://example.test/food/cover-$suffix.jpg?size=large&v=1"
$httpUrl = "$BaseUrl/images/pho.svg?cover=$suffix"
$sideUrl = "$BaseUrl/images/rice.svg?side=$suffix"

try {
    $null = Send-Form "/Language/Set" @{ culture = "en"; returnUrl = "/Dishes/Create" } "/"
    $createForm = Get-Page "/Dishes/Create"
    Check ($createForm.StatusCode -eq 200 -and $createForm.Content -match 'name="CoverImageUrl"' -and $createForm.Content.Contains("Cover image URL")) "Create form exposes cover image URL"
    $invalidCreate = Send-Form "/Dishes/Create" @{Name="";Price="1";PreparationTime="1";CoverImageUrl="javascript:alert(1)";CurrentCoverImageUrl="https://example.test/untrusted-preview.jpg"} "/Dishes/Create"
    Check ($invalidCreate.StatusCode -eq 200 -and $invalidCreate.Content.Contains("HTTP/HTTPS URL") -and -not $invalidCreate.Content.Contains('src="https://example.test/untrusted-preview.jpg"')) "Invalid create URL rejected and preview comes from server"

    $category = Send-Form "/Categories/Create" @{ Name = $categoryName; Description = "Temporary cover URL tests" } "/Categories/Create"
    $categoryPattern = '(?s)<strong>' + [regex]::Escape($categoryName) + '</strong>.*?/Categories/Edit/(\d+)'
    $categoryId = [regex]::Match($category.Content, $categoryPattern).Groups[1].Value
    Check ($category.StatusCode -eq 200 -and -not [string]::IsNullOrEmpty($categoryId)) "Create isolated test category"

    $created = Send-Form "/Dishes/Create" (Dish-Body "0" $coverDishName $httpsUrl) "/Dishes/Create"
    $coverDishId = Track-Dish $created
    Check ($created.StatusCode -eq 200 -and -not [string]::IsNullOrEmpty($coverDishId)) "Create dish with HTTPS Internet image URL"
    Check ((Image-Count $created.Content) -eq 1 -and (Cover-Count $created.Content) -eq 1 -and $created.Content.Contains('src="' + $httpsUrl + '"')) "HTTPS URL creates exactly one cover image"
    $coverImageId = @(Image-Ids $created.Content)[0]
    Check (-not [string]::IsNullOrEmpty($coverImageId)) "Cover image has a persisted ID"

    $editForm = Get-Page "/Dishes/Edit/$coverDishId"
    Check ($editForm.StatusCode -eq 200 -and $editForm.Content.Contains("Cover image URL") -and (Input-Value $editForm.Content "CoverImageUrl") -eq $httpsUrl) "Edit form exposes and prefills current cover URL"
    Check ($editForm.Content.Contains('src="' + $httpsUrl + '"')) "Edit form shows current cover preview"

    $addedSide = Send-Form "/Dishes/AddImage" @{ "Image.DishId" = $coverDishId; "Image.ImageUrl" = $sideUrl; "Image.IsThumbnail" = "false" } "/Dishes/Details/$coverDishId"
    $originalImageIds = @(Image-Ids $addedSide.Content)
    Check ((Image-Count $addedSide.Content) -eq 2 -and (Cover-Count $addedSide.Content) -eq 1) "Additional gallery image keeps one cover"

    $updated = Send-Form "/Dishes/Edit/$coverDishId" (Dish-Body $coverDishId $coverDishName $httpUrl) "/Dishes/Edit/$coverDishId"
    Check ($updated.StatusCode -eq 200 -and $updated.Content.Contains('src="' + $httpUrl + '"')) "Edit accepts absolute HTTP cover URL"
    $updatedIds = @(Image-Ids $updated.Content)
    Check ((Image-Count $updated.Content) -eq 2 -and ($originalImageIds -join ',') -eq ($updatedIds -join ',')) "Cover replacement retains image ID and count"
    Check ((Cover-Count $updated.Content) -eq 1 -and $updated.Content.Contains('src="' + $sideUrl + '"')) "Cover replacement preserves additional image and sole thumbnail"
    $coverImageForm = Get-Page "/Dishes/EditImage/$coverImageId"
    Check ((Input-Value $coverImageForm.Content "ImageUrl") -eq $httpUrl) "Existing cover record stores replacement URL"
    $list = Get-Page "/?CategoryId=$categoryId"
    Check ($list.Content.Contains('src="' + $httpUrl + '"') -and -not $list.Content.Contains('src="' + $httpsUrl + '"')) "Dish list reflects replacement URL"
    $editForm = Get-Page "/Dishes/Edit/$coverDishId"
    Check ((Input-Value $editForm.Content "CoverImageUrl") -eq $httpUrl) "Updated cover URL persists on dish edit form"
    $trimmedEdit = Send-Form "/Dishes/Edit/$coverDishId" (Dish-Body $coverDishId $coverDishName "  $httpUrl  ") "/Dishes/Edit/$coverDishId"
    Check ($trimmedEdit.Content.Contains('src="' + $httpUrl + '"') -and (Input-Value (Get-Page "/Dishes/Edit/$coverDishId").Content "CoverImageUrl") -eq $httpUrl) "URL whitespace is trimmed when saved"

    $blankBody = Dish-Body $coverDishId $coverDishName ""
    $blankBody.Description = "Saved while preserving cover"
    $blank = Send-Form "/Dishes/Edit/$coverDishId" $blankBody "/Dishes/Edit/$coverDishId"
    Check ($blank.Content.Contains("Saved while preserving cover") -and $blank.Content.Contains('src="' + $httpUrl + '"')) "Blank optional cover URL preserves image while saving dish"
    Check ((Image-Count $blank.Content) -eq 2 -and (Cover-Count $blank.Content) -eq 1 -and (@(Image-Ids $blank.Content) -join ',') -eq ($originalImageIds -join ',')) "Blank cover URL keeps image IDs, count and sole thumbnail"

    $invalidName = "RejectedEdit_$suffix"
    $invalid = Send-Form "/Dishes/Edit/$coverDishId" (Dish-Body $coverDishId $invalidName "javascript:alert(1)") "/Dishes/Edit/$coverDishId"
    Check ($invalid.StatusCode -eq 200 -and $invalid.Content.Contains("HTTP/HTTPS URL") -and (Input-Value $invalid.Content "CoverImageUrl") -eq "javascript:alert(1)" -and (Input-Value $invalid.Content "Name") -eq $invalidName) "Invalid English URL shows error and retains entered values"
    $afterInvalid = Get-Page "/Dishes/Edit/$coverDishId"
    Check ((Input-Value $afterInvalid.Content "Name") -eq $coverDishName -and (Input-Value $afterInvalid.Content "CoverImageUrl") -eq $httpUrl -and $afterInvalid.Content.Contains("Saved while preserving cover")) "Invalid URL does not save dish or image changes"

    $null = Send-Form "/Language/Set" @{ culture = "vi"; returnUrl = "/Dishes/Edit/$coverDishId" } "/"
    $viForm = Get-Page "/Dishes/Edit/$coverDishId"
    Check ($viForm.Content.Contains("URL ảnh đại diện") -and $viForm.Content.Contains("ảnh trên mạng")) "Vietnamese edit form explains Internet image URL"
    $invalidVi = Send-Form "/Dishes/Edit/$coverDishId" (Dish-Body $coverDishId $invalidName "ftp://example.test/image.jpg") "/Dishes/Edit/$coverDishId"
    Check ($invalidVi.Content.Contains("Nhập URL HTTP/HTTPS") -and (Input-Value $invalidVi.Content "CoverImageUrl") -eq "ftp://example.test/image.jpg") "Invalid Vietnamese URL shows error and retains input"
    Check ((Input-Value $viForm.Content "Price") -match '^\d+(\.\d+)?$' -and (Input-Value $invalidVi.Content "Price") -match '^\d+(\.\d+)?$') "Vietnamese form keeps browser number input format after URL validation"
    $null = Send-Form "/Language/Set" @{ culture = "en"; returnUrl = "/Dishes/Create" } "/"
    $afterInvalidVi = Get-Page "/Dishes/Edit/$coverDishId"
    Check ((Input-Value $afterInvalidVi.Content "Name") -eq $coverDishName -and (Input-Value $afterInvalidVi.Content "CoverImageUrl") -eq $httpUrl) "Vietnamese invalid URL leaves persisted dish and cover unchanged"

    $emptyCreated = Send-Form "/Dishes/Create" (Dish-Body "0" $emptyDishName "") "/Dishes/Create"
    $emptyDishId = Track-Dish $emptyCreated
    Check (-not [string]::IsNullOrEmpty($emptyDishId) -and (Image-Count $emptyCreated.Content) -eq 0 -and $emptyCreated.Content.Contains("No images yet")) "Create without optional cover URL keeps no-image state"
    $emptyEdit = Get-Page "/Dishes/Edit/$emptyDishId"
    Check ((Input-Value $emptyEdit.Content "CoverImageUrl") -eq "") "Dish without cover exposes blank edit URL"
    $newCover = Send-Form "/Dishes/Edit/$emptyDishId" (Dish-Body $emptyDishId $emptyDishName $httpsUrl) "/Dishes/Edit/$emptyDishId"
    Check ($newCover.Content.Contains('src="' + $httpsUrl + '"') -and (Image-Count $newCover.Content) -eq 1 -and (Cover-Count $newCover.Content) -eq 1) "Editing dish without cover creates one HTTPS cover"
    $newCoverIds = @(Image-Ids $newCover.Content)
    $sameCover = Send-Form "/Dishes/Edit/$emptyDishId" (Dish-Body $emptyDishId $emptyDishName $httpsUrl) "/Dishes/Edit/$emptyDishId"
    Check ((Image-Count $sameCover.Content) -eq 1 -and (Cover-Count $sameCover.Content) -eq 1 -and (@(Image-Ids $sameCover.Content) -join ',') -eq ($newCoverIds -join ',')) "Repeated save updates cover without duplicate thumbnail"
    $finalEdit = Get-Page "/Dishes/Edit/$emptyDishId"
    $finalList = Get-Page "/?CategoryId=$categoryId"
    Check ((Input-Value $finalEdit.Content "CoverImageUrl") -eq $httpsUrl -and $finalList.Content.Contains('src="' + $httpsUrl + '"')) "New cover persists in Edit and dish list"
} finally {
    # Delete dishes first so the category is no longer in use. Attempt every
    # cleanup even if an earlier assertion or cleanup request failed.
    foreach ($dishId in $dishIds) {
        try {
            $null = Send-Form "/Dishes/Delete/$dishId" @{ id = $dishId } "/Dishes/Delete/$dishId"
            $deleted = Get-Page "/Dishes/Details/$dishId"
            if ($deleted.StatusCode -ne 404) { Write-Warning "Temporary dish $dishId could not be deleted." }
        } catch { Write-Warning "Cleanup failed for dish ${dishId}: $_" }
    }
    if (-not [string]::IsNullOrEmpty($categoryId)) {
        try {
            $null = Send-Form "/Categories/Delete/$categoryId" @{ id = $categoryId } "/Categories/Delete/$categoryId"
            $deleted = Get-Page "/Categories/Edit/$categoryId"
            if ($deleted.StatusCode -ne 404) { Write-Warning "Temporary category $categoryId could not be deleted." }
        } catch { Write-Warning "Cleanup failed for category ${categoryId}: $_" }
    }
}

Write-Host "All $passed cover URL checks passed."
