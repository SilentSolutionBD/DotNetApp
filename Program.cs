global using System.Collections.Generic;
global using System.Linq;
global using InventoryBeginners.Data;
global using InventoryBeginners.Interfaces;
global using InventoryBeginners.Models;
global using Microsoft.EntityFrameworkCore;
global using CodesByAniz.Tools;
global using System.ComponentModel.DataAnnotations;

global using Microsoft.AspNetCore.Mvc;
global using System;
global using Microsoft.AspNetCore.Authorization;

global using Microsoft.Extensions.Logging;
global using System.Diagnostics;

global using Microsoft.AspNetCore.Mvc.Rendering;
global using Microsoft.AspNetCore.Hosting;
global using System.IO;

global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.AspNetCore.Http;
global using System.ComponentModel.DataAnnotations.Schema;
global using Microsoft.AspNetCore.Identity;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;



using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using InventoryBeginners.Repositories;
using System.Configuration;
using Microsoft.Extensions.Configuration;



var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddScoped<IUnit, UnitRepository>();

builder.Services.AddScoped<IProduct, ProductRepo>();

builder.Services.AddScoped<ISupplier, SupplierRepo>();

builder.Services.AddScoped<ICategory, CategoryRepo>();
builder.Services.AddScoped<IBrand, BrandRepo>();
builder.Services.AddScoped<IProductProfile, ProductProfileRepo>();
builder.Services.AddScoped<IProductGroup, ProductGroupRepo>();

builder.Services.AddScoped<ICurrency, CurrencyRepo>();

builder.Services.AddScoped<IPurchaseOrder, PurchaseOrderRepo>();



//services.AddScoped<IProductAttribute, ProductAttributeRepo>();

//builder.Services.AddDbContext<InventoryContext>(options =>
//options.UseSqlServer(builder.Configuration.GetSection("ConnectionStrings:dbconn").Value));

builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("dbconn"),
        new MySqlServerVersion(new Version(8, 0, 42)) // <-- use Mysql version here
    ));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<InventoryContext>();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryContext>();
    dbContext.Database.Migrate();
}

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapRazorPages();

});

app.Run();





