using SiteViewer.WWW.AppService;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.UseAppConfig();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddProblemDetailsDeveloperPageExceptionFilter();
builder.Services.UseApiCrosPolicy();

var app = builder.Build();

app.UseFileRepRewrite();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseCors("ApiCorsPolicy");

app.UseAuthorization();

app.Run();