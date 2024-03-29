using API.Common.Accounts.Validators;
using API.Hubs;
using API.Providers;
using BLL.Common.Constants;
using BLL.Common.Mapper;
using BLL.Common.Validators;
using BLL.Services.Abstractions;
using BLL.Services.Implementations;
using DAL;
using DAL.Entities;
using DAL.Repositories;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

RegisterServices(builder);

var app = builder.Build();

Configure(app);

app.Run();

void RegisterServices(WebApplicationBuilder app)
{
    app.Services.AddMvc().AddNewtonsoftJson();
    app.Services.AddEndpointsApiExplorer();
    app.Services.AddSwaggerGen(option =>
    {
        option.SwaggerDoc("v1", new OpenApiInfo { Title = "CrowMessenger Open API", Version = "v1" });
        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    app.Services.AddDbContext<DefaultDbContext>(options =>
    {
        options.UseSqlServer(app.Configuration.GetConnectionString("DefaultConnection"));
    });

    app.Services.AddDefaultIdentity<AppUser>()
      .AddRoles<IdentityRole>()
      .AddEntityFrameworkStores<DefaultDbContext>();

    var dbBuilder = app.Services.AddIdentityCore<AppUser>(o =>
    {
        o.Password.RequireDigit = false;
        o.Password.RequireLowercase = false;
        o.Password.RequireUppercase = false;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequiredLength = 6;
    });

    dbBuilder = new IdentityBuilder(dbBuilder.UserType, typeof(IdentityRole), dbBuilder.Services);
    dbBuilder.AddEntityFrameworkStores<DefaultDbContext>().AddDefaultTokenProviders();

    app.Services.AddAutoMapper(cfg => cfg.AddProfile(new AutoMapperProfileConfiguration()));

    app.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowAnyOrigin();
        });

        options.AddPolicy("signalr", builder => builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(hostName => true
            ));
    });

    app.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
    app.Services.AddFluentValidationAutoValidation();
    app.Services.AddFluentValidationClientsideAdapters();

    app.Services.AddSignalR(options => { options.EnableDetailedErrors = true; });

    app.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

    app.Services.AddAuthentication();
    app.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(options =>
          {
              options.TokenValidationParameters = new()
              {
                  ValidateIssuer = true,
                  ValidateAudience = true,
                  ValidateLifetime = true,
                  ValidateIssuerSigningKey = true,
                  ValidIssuer = app.Configuration["Jwt:Issuer"],
                  ValidAudience = app.Configuration["Jwt:Audience"],
                  IssuerSigningKey = new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(app.Configuration["Jwt:Key"] ?? "JustStrongSecret"))
              };

              options.Events = new JwtBearerEvents
              {
                  OnMessageReceived = context =>
                  {
                      var accessToken = context.Request.Query["access_token"];

                      var path = context.HttpContext.Request.Path;
                      if (!string.IsNullOrEmpty(accessToken) &&
                          path.StartsWithSegments("/api/live"))
                      {
                          var token = accessToken.ToString();

                          context.Token = token.Split(' ').LastOrDefault();
                      }

                      return Task.CompletedTask;
                  }
              };
          });

    app.Services.AddTransient<IUserValidator<AppUser>, OptionalEmailUserValidator<AppUser>>();

    app.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    app.Services.AddScoped<IAccountsService, AccountsService>();
    app.Services.AddScoped<IJWTService, JWTService>();
    app.Services.AddScoped<IAuthService, AuthService>();
    app.Services.AddScoped<IChatService, ChatService>();
    app.Services.AddScoped<IMessageService, MessageService>();
    app.Services.AddScoped<IFileService, FileService>();
    app.Services.AddScoped<IUsersService, UsersService>();

    app.Services.AddHttpContextAccessor();
}

void Configure(WebApplication app)
{
    app.UseSwagger();
    app.UseSwaggerUI();

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
        db.Database.EnsureCreated();
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<ChatHub>("/api/live/chat");

    var path = Path.Combine(Directory.GetCurrentDirectory(), FileConstants.StaticFilesFolder);
    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

    var usersFiles = Path.Combine(path, FileConstants.UsersFiles);
    if (!Directory.Exists(usersFiles)) Directory.CreateDirectory(usersFiles);

    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".json"] = "application/json";
    provider.Mappings[".webmanifest"] = "application/manifest+json";

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(path),
        RequestPath = new PathString("/static"),
        ContentTypeProvider = provider
    });
}