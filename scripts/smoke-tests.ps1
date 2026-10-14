param([string]$BaseUrl = "http://localhost:5080")
$ErrorActionPreference = "Stop"
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$passed = 0
function Check($condition, $message) {
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
    if (-not $match.Success) { throw "Antiforgery token missing" }
    $body["__RequestVerificationToken"] = [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
    $response = Invoke-WebRequest "$BaseUrl$path" -Method Post -Body $body -WebSession $session -SkipHttpErrorCheck
    [pscustomobject]@{ StatusCode = $response.StatusCode; Content = [System.Net.WebUtility]::HtmlDecode($response.Content) }
}
$homePage = Get-Page "/"
Check ($homePage.StatusCode -eq 200 -and $homePage.Content.Contains('lang="en"')) "English is the default"
Check (([regex]::Matches($homePage.Content, '<article class="dish-card">')).Count -ge 5) "At least five seeded dishes"
Check ($homePage.Content.Contains("Unavailable") -and $homePage.Content.Contains("Quick") -and $homePage.Content.Contains("Regular")) "Statuses and calculated time labels"
$categories = Get-Page "/Categories"
$categoryIds = [regex]::Matches($categories.Content, '/Categories/Edit/(\d+)') | ForEach-Object { [int]$_.Groups[1].Value }
Check ($categoryIds.Count -ge 3) "Three seeded categories"
$mainMatch = [regex]::Match($homePage.Content, '<option value="(\d+)">Main courses</option>')
$mainId = $mainMatch.Groups[1].Value
$filterPath = "/?Search=Ph&CategoryId=$mainId&Status=available&MinPrice=60000&MaxPrice=70000&Sort=price_desc"
$filtered = Get-Page $filterPath
Check (([regex]::Matches($filtered.Content, '<article class="dish-card">')).Count -eq 1) "Combined database filters return one dish"
Check ($filtered.Content -match 'value="price_desc" selected="selected"') "Sort selection retained"
$vi = Send-Form "/Language/Set" @{ culture="vi"; returnUrl=$filterPath } $filterPath
Check ($vi.Content.Contains('lang="vi"') -and $vi.Content.Contains("Lọc kết quả")) "Vietnamese switch"
Check (([regex]::Matches($vi.Content, '<article class="dish-card">')).Count -eq 1) "Language switch preserves filters"
$createVi = Get-Page "/Dishes/Create"
Check ($createVi.Content.Contains('lang="vi"') -and $createVi.Content.Contains("Thêm món ăn")) "Language persists across navigation"
$invalidVi = Send-Form "/Dishes/Create" @{Name="";Price="0";PreparationTime="0";DishCategoryId="999999";IsAvailable="true"} "/Dishes/Create"
Check ($invalidVi.StatusCode -eq 200 -and $invalidVi.Content.Contains("không được để trống") -and $invalidVi.Content.Contains("lớn hơn 0") -and $invalidVi.Content.Contains("không tồn tại")) "Vietnamese validation for blank name, price, time, category"
$nullVi = Send-Form "/Dishes/Create" @{} "/Dishes/Create"
Check ($nullVi.StatusCode -eq 200 -and $nullVi.Content.Contains("không được để trống")) "Missing fields handled without server error"
$en = Send-Form "/Language/Set" @{culture="en";returnUrl="/"} "/"
$invalidEn = Send-Form "/Dishes/Create" @{Name="";Price="0";PreparationTime="0";DishCategoryId="999999"} "/Dishes/Create"
Check ($invalidEn.Content.Contains("This field is required.") -and $invalidEn.Content.Contains("greater than zero")) "English validation"
$badRange = Get-Page "/?MinPrice=70000&MaxPrice=10000"
Check ($badRange.StatusCode -eq 200 -and $badRange.Content.Contains("minimum must not exceed")) "Invalid price range"
$badNumber = Get-Page "/?MinPrice=abc"
Check ($badNumber.StatusCode -eq 200 -and $badNumber.Content.Contains("valid number")) "Invalid numeric filter"
$empty = Get-Page "/?Search=zzzz_no_match"
Check ($empty.Content.Contains("No dishes found")) "Empty search state"
$available = Get-Page "/?Status=available"
Check (-not $available.Content.Contains('status-badge unavailable')) "Available filter excludes stopped dishes"
foreach ($path in @("/Dishes/Details/999999","/Dishes/Edit/999999","/Categories/Edit/999999","/Categories/Delete/999999")) {
    $missing = Get-Page $path
    Check ($missing.StatusCode -eq 404 -and $missing.Content.Contains("does not exist")) "Missing ID: $path"
}
$usedCategory = Send-Form "/Categories/Delete/$mainId" @{id=$mainId} "/Categories/Delete/$mainId"
Check ($usedCategory.StatusCode -eq 200 -and $usedCategory.Content.Contains("cannot be deleted")) "Used category deletion blocked"
$name = "SmokeTest_" + [Guid]::NewGuid().ToString("N").Substring(0,8)
$created = Send-Form "/Dishes/Create" @{Name=$name;Price="12345";PreparationTime="15";DishCategoryId=$mainId;IsAvailable="true";Description="Temporary integration test"} "/Dishes/Create"
Check ($created.StatusCode -eq 200 -and $created.Content.Contains($name) -and $created.Content.Contains("No images yet")) "Create dish and no-image detail"
$dishId = [regex]::Match($created.Content, '/Dishes/Edit/(\d+)').Groups[1].Value
Check (-not [string]::IsNullOrEmpty($dishId)) "Created dish ID"
try {
    $duplicate = Send-Form "/Dishes/Create" @{Name=$name;Price="100";PreparationTime="2";DishCategoryId=$mainId} "/Dishes/Create"
    Check ($duplicate.Content.Contains("already exists in this category")) "Duplicate name rejected within category"
    $edited = Send-Form "/Dishes/Edit/$dishId" @{Id=$dishId;Name=$name;Price="23456";PreparationTime="16";DishCategoryId=$mainId;IsAvailable="false"} "/Dishes/Edit/$dishId"
    Check ($edited.Content.Contains("Unavailable") -and $edited.Content.Contains("Regular")) "Edit saves availability and computed classification"
    $null = Send-Form "/Language/Set" @{culture="vi";returnUrl="/Dishes/Edit/$dishId"} "/"
    $decimalEdit = Send-Form "/Dishes/Edit/$dishId" @{Id=$dishId;Name=$name;Price="12345.67";PreparationTime="16";DishCategoryId=$mainId;IsAvailable="false"} "/Dishes/Edit/$dishId"
    Check ($decimalEdit.Content.Contains("12.345,67")) "Decimal price saved correctly in Vietnamese"
    $decimalFilter = Get-Page "/?Search=$name&MinPrice=12345.66&MaxPrice=12345.68"
    Check (([regex]::Matches($decimalFilter.Content, '<article class="dish-card">')).Count -eq 1) "Decimal price filters in Vietnamese"
    $null = Send-Form "/Language/Set" @{culture="en";returnUrl="/Dishes/Details/$dishId"} "/"
    $invalidUrl = Send-Form "/Dishes/AddImage" @{"Image.DishId"=$dishId;"Image.ImageUrl"="javascript:alert(1)"} "/Dishes/Details/$dishId"
    Check ($invalidUrl.Content.Contains("HTTP/HTTPS URL")) "Invalid image URL rejected"
    $image1 = Send-Form "/Dishes/AddImage" @{"Image.DishId"=$dishId;"Image.ImageUrl"="$BaseUrl/images/pho.svg";"Image.IsThumbnail"="true"} "/Dishes/Details/$dishId"
    Check ($image1.Content.Contains("src=`"$BaseUrl/images/pho.svg`"")) "Add image by absolute HTTP URL"
    $image2 = Send-Form "/Dishes/AddImage" @{"Image.DishId"=$dishId;"Image.ImageUrl"="/images/rice.svg";"Image.IsThumbnail"="true"} "/Dishes/Details/$dishId"
    Check (([regex]::Matches($image2.Content, '<article class="image-card">')).Count -eq 2) "Multiple images displayed"
    $imageIds = [regex]::Matches($image2.Content, 'action="/Dishes/DeleteImage"[^>]*>\s*<input name="id" type="hidden" value="(\d+)"') | ForEach-Object { $_.Groups[1].Value }
    Check ($imageIds.Count -eq 2) "Image action IDs available"
    $switched = Send-Form "/Dishes/SetThumbnail" @{id=$imageIds[1]} "/Dishes/Details/$dishId"
    Check ($switched.Content.Contains("Cover image updated.") -and ([regex]::Matches($switched.Content, 'time-badge quick">Cover image')).Count -eq 1) "Cover switched with one thumbnail"
    $editImageId = $imageIds[1]
    $editImage = Get-Page "/Dishes/EditImage/$editImageId"
    Check ($editImage.StatusCode -eq 200 -and $editImage.Content.Contains("Edit image URL")) "Image URL edit form"
    $imageEdited = Send-Form "/Dishes/EditImage/$editImageId" @{Id=$editImageId;ImageUrl="$BaseUrl/images/dessert.svg";DishId="999999";IsThumbnail="false"} "/Dishes/EditImage/$editImageId"
    Check ($imageEdited.Content.Contains("Image URL updated.") -and $imageEdited.Content.Contains("src=`"$BaseUrl/images/dessert.svg`"")) "Edit saves new image URL"
    Check (([regex]::Matches($imageEdited.Content, '<article class="image-card">')).Count -eq 2 -and ([regex]::Matches($imageEdited.Content, 'time-badge quick">Cover image')).Count -eq 1) "URL edit preserves image count, dish and cover"
    $editCover = Get-Page "/Dishes/EditImage/$editImageId"
    Check ($editCover.Content.Contains('value="' + $BaseUrl + '/images/dessert.svg"') -and $editCover.Content.Contains('time-badge quick">Cover image')) "Saved URL persists on image edit page"
    $badImageEdit = Send-Form "/Dishes/EditImage/$editImageId" @{Id=$editImageId;ImageUrl="javascript:alert(1)"} "/Dishes/EditImage/$editImageId"
    Check ($badImageEdit.StatusCode -eq 200 -and $badImageEdit.Content.Contains("HTTP/HTTPS URL") -and $badImageEdit.Content.Contains('value="javascript:alert(1)"')) "Invalid edited URL keeps input and shows validation"
    $blankImageEdit = Send-Form "/Dishes/EditImage/$editImageId" @{Id=$editImageId;ImageUrl=""} "/Dishes/EditImage/$editImageId"
    Check ($blankImageEdit.StatusCode -eq 200 -and $blankImageEdit.Content.Contains("This field is required.")) "Blank edited URL rejected"
    $nullImageEdit = Send-Form "/Dishes/EditImage/$editImageId" @{Id=$editImageId} "/Dishes/EditImage/$editImageId"
    Check ($nullImageEdit.StatusCode -eq 200 -and $nullImageEdit.Content.Contains("This field is required.")) "Missing edited URL handled"
    $null = Send-Form "/Language/Set" @{culture="vi";returnUrl="/Dishes/EditImage/$editImageId"} "/"
    $viImageEdit = Send-Form "/Dishes/EditImage/$editImageId" @{Id=$editImageId;ImageUrl="javascript:alert(1)"} "/Dishes/EditImage/$editImageId"
    Check ($viImageEdit.Content.Contains("Sửa ảnh bằng URL") -and $viImageEdit.Content.Contains("Nhập URL HTTP/HTTPS")) "Vietnamese image edit UI and validation"
    $null = Send-Form "/Language/Set" @{culture="en";returnUrl="/Dishes/Details/$dishId"} "/"
    $afterInvalid = Get-Page "/Dishes/EditImage/$editImageId"
    Check ($afterInvalid.Content.Contains('value="' + $BaseUrl + '/images/dessert.svg"')) "Invalid URL changes do not update database"
    $mismatchImage = Send-Form "/Dishes/EditImage/$editImageId" @{Id="999999";ImageUrl="/images/table.svg"} "/Dishes/EditImage/$editImageId"
    Check ($mismatchImage.StatusCode -eq 404) "Mismatched image ID handled"
    $missingImageGet = Get-Page "/Dishes/EditImage/999999"
    $missingImagePost = Send-Form "/Dishes/EditImage/999999" @{Id="999999";ImageUrl="/images/table.svg"} "/Dishes/Details/$dishId"
    Check ($missingImageGet.StatusCode -eq 404 -and $missingImagePost.StatusCode -eq 404) "Missing image ID on edit GET and POST"
    $missingImage = Send-Form "/Dishes/SetThumbnail" @{id="999999"} "/Dishes/Details/$dishId"
    Check ($missingImage.StatusCode -eq 404) "Missing image ID handled"
    foreach ($imageId in $imageIds) { $null = Send-Form "/Dishes/DeleteImage" @{id=$imageId} "/Dishes/Details/$dishId" }
    $noImages = Get-Page "/Dishes/Details/$dishId"
    Check ($noImages.Content.Contains("No images yet")) "Delete images restores empty state"
} finally {
    $null = Send-Form "/Dishes/Delete/$dishId" @{id=$dishId} "/Dishes/Delete/$dishId"
}
$newCategory = Send-Form "/Categories/Create" @{Name=$name;Description="Temporary category"} "/Categories/Create"
$tempId = [regex]::Match($newCategory.Content, '(?s)<strong>' + $name + '</strong>.*?/Categories/Edit/(\d+)').Groups[1].Value
Check (-not [string]::IsNullOrEmpty($tempId)) "Create category"
try {
    $catEdit = Send-Form "/Categories/Edit/$tempId" @{Id=$tempId;Name=$name;Description="Updated category"} "/Categories/Edit/$tempId"
    Check ($catEdit.Content.Contains("Updated category")) "Edit category"
} finally {
    $catDeleted = Send-Form "/Categories/Delete/$tempId" @{id=$tempId} "/Categories/Delete/$tempId"
    Check (-not $catDeleted.Content.Contains($name)) "Unused category can be deleted"
}
$csrf = Invoke-WebRequest "$BaseUrl/Dishes/Create" -Method Post -Body @{Name="Forbidden"} -WebSession $session -SkipHttpErrorCheck
Check ($csrf.StatusCode -eq 400) "Antiforgery protection"
Write-Host "All $passed checks passed."
