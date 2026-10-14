using FoodManager_BIT240194.Data;
using FoodManager_BIT240194.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.ModelBinderProviders.Insert(0, new InvariantDecimalBinderProvider());
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, field) => UiText.Message("InvalidValue"));
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(field => UiText.Message("InvalidNumber"));
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(value => UiText.Message("Required"));
});
builder.Services.AddScoped<UiText>();
builder.Services.AddDbContext<FoodDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("FoodDatabase")));
var app = builder.Build();
if (args.Contains("--export-schema"))
{
    using var scope = app.Services.CreateScope();
    File.WriteAllText("database/BIT240194_schema.sql", scope.ServiceProvider.GetRequiredService<FoodDbContext>().Database.GenerateCreateScript());
    return;
}
var cultures = new[] { new CultureInfo("en"), new CultureInfo("vi") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = cultures, SupportedUICultures = cultures,
    RequestCultureProviders = new List<IRequestCultureProvider> { new CookieRequestCultureProvider() }
});
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Dishes}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FoodDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db);
}
app.Run();
