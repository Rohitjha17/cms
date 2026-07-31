using System;
using System.Collections.Generic;
using Cms.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AspNetRoles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AspNetUsers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                LogoUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Tenants", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Sites",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                SiteKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                WebsiteType = table.Column<int>(type: "int", nullable: false),
                IsDefault = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sites", x => x.Id);
                table.ForeignKey(
                    name: "FK_Sites_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TenantDomains",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DomainName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TenantDomains", x => x.Id);
                table.ForeignKey(
                    name: "FK_TenantDomains_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "HomePageSections",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SectionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                SubTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ButtonText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ButtonLink = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                BackgroundImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                JsonData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HomePageSections", x => x.Id);
                table.ForeignKey(
                    name: "FK_HomePageSections_Sites_SiteId",
                    column: x => x.SiteId,
                    principalTable: "Sites",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_HomePageSections_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "MediaFiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                MediaType = table.Column<int>(type: "int", nullable: false),
                Folder = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MediaFiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_MediaFiles_Sites_SiteId",
                    column: x => x.SiteId,
                    principalTable: "Sites",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MediaFiles_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Menus",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Menus", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Pages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Pages", x => x.Id));

        migrationBuilder.CreateTable(
            name: "SeoSettings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MetaTitle = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                MetaKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                OgImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CanonicalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_SeoSettings", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_AspNetRoleClaims_RoleId", table: "AspNetRoleClaims", column: "RoleId");
        migrationBuilder.CreateIndex(name: "RoleNameIndex", table: "AspNetRoles", column: "NormalizedName", unique: true, filter: "[NormalizedName] IS NOT NULL");
        migrationBuilder.CreateIndex(name: "IX_AspNetUserClaims_UserId", table: "AspNetUserClaims", column: "UserId");
        migrationBuilder.CreateIndex(name: "IX_AspNetUserLogins_UserId", table: "AspNetUserLogins", column: "UserId");
        migrationBuilder.CreateIndex(name: "IX_AspNetUserRoles_RoleId", table: "AspNetUserRoles", column: "RoleId");
        migrationBuilder.CreateIndex(name: "EmailIndex", table: "AspNetUsers", column: "NormalizedEmail");
        migrationBuilder.CreateIndex(name: "UserNameIndex", table: "AspNetUsers", column: "NormalizedUserName", unique: true, filter: "[NormalizedUserName] IS NOT NULL");
        migrationBuilder.CreateIndex(name: "IX_HomePageSections_SiteId", table: "HomePageSections", column: "SiteId");
        migrationBuilder.CreateIndex(name: "IX_HomePageSections_TenantId_SiteId_DisplayOrder", table: "HomePageSections", columns: new[] { "TenantId", "SiteId", "DisplayOrder" });
        migrationBuilder.CreateIndex(name: "IX_HomePageSections_TenantId_SiteId_SectionKey", table: "HomePageSections", columns: new[] { "TenantId", "SiteId", "SectionKey" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_MediaFiles_SiteId", table: "MediaFiles", column: "SiteId");
        migrationBuilder.CreateIndex(name: "IX_MediaFiles_TenantId_SiteId_StorageKey", table: "MediaFiles", columns: new[] { "TenantId", "SiteId", "StorageKey" });
        migrationBuilder.CreateIndex(name: "IX_Menus_TenantId_SiteId", table: "Menus", columns: new[] { "TenantId", "SiteId" });
        migrationBuilder.CreateIndex(name: "IX_Pages_TenantId_SiteId_Slug", table: "Pages", columns: new[] { "TenantId", "SiteId", "Slug" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_SeoSettings_TenantId_SiteId", table: "SeoSettings", columns: new[] { "TenantId", "SiteId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Sites_TenantId_SiteKey", table: "Sites", columns: new[] { "TenantId", "SiteKey" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_TenantDomains_DomainName", table: "TenantDomains", column: "DomainName", unique: true);
        migrationBuilder.CreateIndex(name: "IX_TenantDomains_TenantId", table: "TenantDomains", column: "TenantId");
        migrationBuilder.CreateIndex(name: "IX_Tenants_Code", table: "Tenants", column: "Code", unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AspNetRoleClaims");
        migrationBuilder.DropTable(name: "AspNetUserClaims");
        migrationBuilder.DropTable(name: "AspNetUserLogins");
        migrationBuilder.DropTable(name: "AspNetUserRoles");
        migrationBuilder.DropTable(name: "AspNetUserTokens");
        migrationBuilder.DropTable(name: "HomePageSections");
        migrationBuilder.DropTable(name: "MediaFiles");
        migrationBuilder.DropTable(name: "Menus");
        migrationBuilder.DropTable(name: "Pages");
        migrationBuilder.DropTable(name: "SeoSettings");
        migrationBuilder.DropTable(name: "TenantDomains");
        migrationBuilder.DropTable(name: "AspNetRoles");
        migrationBuilder.DropTable(name: "AspNetUsers");
        migrationBuilder.DropTable(name: "Sites");
        migrationBuilder.DropTable(name: "Tenants");
    }
}
