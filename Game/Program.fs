module Game.Program

open System
open System.IO
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins("http://localhost:8080")
        .WithOrigins("http://localhost:5000")
        .WithOrigins("https://localhost:5001")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        |> ignore

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddAuthentication(fun options ->
                options.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
                options.DefaultSignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
                options.DefaultChallengeScheme <- "Google"
            )
            .AddCookie()
            .AddGoogle(fun options -> 
                options.ClientId <- System.Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
                options.ClientSecret <- System.Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
            )
            |> ignore
    services.AddGiraffe() |> ignore
    services.AddSignalR() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
        .AddConsole()
        .AddDebug() |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    match env.IsDevelopment() with
        | true  -> app.UseDeveloperExceptionPage() |> ignore
        | false -> 
            app.UseGiraffeErrorHandler(errorHandler)
                .UseCors(configureCors)
                .UseHttpsRedirection()
                .UseStaticFiles()
                .UseAuthentication()
                .UseSignalR(fun routes -> routes.MapHub<Game.Hubs.GameHub>(PathString "/gamehub"))
                .UseGiraffe(Game.App.app)
    

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0