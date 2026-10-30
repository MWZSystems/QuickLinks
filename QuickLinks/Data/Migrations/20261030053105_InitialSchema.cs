using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuickLinks.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CT_Roles",
                columns: table => new
                {
                    PKRole = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Roles", x => x.PKRole);
                });

            migrationBuilder.CreateTable(
                name: "CT_Settings",
                columns: table => new
                {
                    PKSetting = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingOrder = table.Column<int>(type: "int", nullable: false),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Settings", x => x.PKSetting);
                });

            migrationBuilder.CreateTable(
                name: "CT_Sites",
                columns: table => new
                {
                    PKSite = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FKUserCreator = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Sites", x => x.PKSite);
                });

            migrationBuilder.CreateTable(
                name: "SC_Users",
                columns: table => new
                {
                    PKUser = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FKRole = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SC_Users", x => x.PKUser);
                    table.ForeignKey(
                        name: "FK_SC_Users_CT_Roles",
                        column: x => x.FKRole,
                        principalTable: "CT_Roles",
                        principalColumn: "PKRole");
                });

            migrationBuilder.CreateTable(
                name: "CT_Advices",
                columns: table => new
                {
                    PkAdvice = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Advices", x => x.PkAdvice);
                    table.ForeignKey(
                        name: "FK_CT_Advices_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.CreateTable(
                name: "CT_Applications",
                columns: table => new
                {
                    PKApplication = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApplicationSME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationUserManual = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ApplicationLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApplicationImage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Applications", x => x.PKApplication);
                    table.ForeignKey(
                        name: "FK_CT_Applications_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.CreateTable(
                name: "CT_Buildings",
                columns: table => new
                {
                    PKBuilding = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FKSite = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Buildings", x => x.PKBuilding);
                    table.ForeignKey(
                        name: "FK_CT_Buildings_CT_Sites",
                        column: x => x.FKSite,
                        principalTable: "CT_Sites",
                        principalColumn: "PKSite");
                    table.ForeignKey(
                        name: "FK_CT_Buildings_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.CreateTable(
                name: "CT_Categories",
                columns: table => new
                {
                    PKCategory = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false, defaultValue: "#0D4D84"),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Categories", x => x.PKCategory);
                    table.ForeignKey(
                        name: "FK_CT_Categories_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.CreateTable(
                name: "CT_Groups",
                columns: table => new
                {
                    PKGroup = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Groups", x => x.PKGroup);
                    table.ForeignKey(
                        name: "FK_CT_Groups_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.CreateTable(
                name: "CT_Carousel",
                columns: table => new
                {
                    PKCarousel = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FKApplication = table.Column<int>(type: "int", nullable: false),
                    ImageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Carousel", x => x.PKCarousel);
                    table.ForeignKey(
                        name: "FK_CT_Carousel_CT_Applications",
                        column: x => x.FKApplication,
                        principalTable: "CT_Applications",
                        principalColumn: "PKApplication");
                    table.ForeignKey(
                        name: "FK_CT_Carousel_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.CreateTable(
                name: "CT_Maintenance",
                columns: table => new
                {
                    PkMaintenance = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FkApplication = table.Column<int>(type: "int", nullable: false),
                    CustomMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    DisableApplication = table.Column<bool>(type: "bit", nullable: false),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_Maintenance", x => x.PkMaintenance);
                    table.ForeignKey(
                        name: "FK_CT_Maintenance_CT_Applications",
                        column: x => x.FkApplication,
                        principalTable: "CT_Applications",
                        principalColumn: "PKApplication");
                    table.ForeignKey(
                        name: "FK_CT_Maintenance_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.CreateTable(
                name: "SY_ApplicationLocation",
                columns: table => new
                {
                    PKApplicationSite = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FKApplication = table.Column<int>(type: "int", nullable: false),
                    FKBuilding = table.Column<int>(type: "int", nullable: false),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SY_ApplicationLocation", x => x.PKApplicationSite);
                    table.ForeignKey(
                        name: "FK_SY_ApplicationLocation_CT_Applications",
                        column: x => x.FKApplication,
                        principalTable: "CT_Applications",
                        principalColumn: "PKApplication");
                    table.ForeignKey(
                        name: "FK_SY_ApplicationLocation_CT_Buildings",
                        column: x => x.FKBuilding,
                        principalTable: "CT_Buildings",
                        principalColumn: "PKBuilding");
                    table.ForeignKey(
                        name: "FK_SY_ApplicationLocation_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.CreateTable(
                name: "CT_CategoryApplications",
                columns: table => new
                {
                    PKCategoryApplication = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FKCategory = table.Column<int>(type: "int", nullable: false),
                    FKApplication = table.Column<int>(type: "int", nullable: false),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_CategoryApplications", x => x.PKCategoryApplication);
                    table.ForeignKey(
                        name: "FK_CT_CategoryApplications_CT_Applications",
                        column: x => x.FKApplication,
                        principalTable: "CT_Applications",
                        principalColumn: "PKApplication");
                    table.ForeignKey(
                        name: "FK_CT_CategoryApplications_CT_Categories",
                        column: x => x.FKCategory,
                        principalTable: "CT_Categories",
                        principalColumn: "PKCategory");
                    table.ForeignKey(
                        name: "FK_CT_CategoryApplications_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.CreateTable(
                name: "SY_ApplicationGroups",
                columns: table => new
                {
                    PKApplicationGroup = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FKApplication = table.Column<int>(type: "int", nullable: false),
                    FKGroup = table.Column<int>(type: "int", nullable: false),
                    FKUserUpdater = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SY_ApplicationGroups", x => x.PKApplicationGroup);
                    table.ForeignKey(
                        name: "FK_SY_ApplicationGroups_CT_Applications",
                        column: x => x.FKApplication,
                        principalTable: "CT_Applications",
                        principalColumn: "PKApplication");
                    table.ForeignKey(
                        name: "FK_SY_ApplicationGroups_CT_Groups",
                        column: x => x.FKGroup,
                        principalTable: "CT_Groups",
                        principalColumn: "PKGroup");
                    table.ForeignKey(
                        name: "FK_SY_ApplicationGroups_SC_Users",
                        column: x => x.FKUserUpdater,
                        principalTable: "SC_Users",
                        principalColumn: "PKUser");
                });

            migrationBuilder.InsertData(
                table: "CT_Roles",
                columns: new[] { "PKRole", "Available", "LastUpdated", "Name", "FKUserUpdater" },
                values: new object[] { 1, true, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Administrador", 1 });

            migrationBuilder.InsertData(
                table: "CT_Settings",
                columns: new[] { "PKSetting", "Available", "LastUpdated", "SettingName", "SettingOrder", "FKUserUpdater", "SettingValue" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "APP_NAME", 1, 1, "QuickLinks" },
                    { 2, true, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "COMPANY_NAME", 2, 1, "" },
                    { 3, true, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "PRIMARY_COLOR", 3, 1, "#0D4D84" },
                    { 4, true, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "LOGO_IMAGE", 4, 1, "" },
                    { 5, true, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "LINK_VALIDATION_REGEX", 5, 1, "^(https?://).+" },
                    { 6, true, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "TIME_ZONE", 6, 1, "America/Mexico_City" }
                });

            migrationBuilder.InsertData(
                table: "SC_Users",
                columns: new[] { "PKUser", "AccessFailedCount", "Available", "LastLogin", "LastUpdated", "LockoutEnd", "Name", "PasswordHash", "FKRole", "FKUserUpdater", "UserName" },
                values: new object[] { 1, 0, true, null, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), null, "Administrador", null, 1, 1, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_CT_Advices_FKUserUpdater",
                table: "CT_Advices",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "UX_CT_Advices_Title",
                table: "CT_Advices",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CT_Applications_FKUserUpdater",
                table: "CT_Applications",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "UX_CT_Applications_ApplicationName",
                table: "CT_Applications",
                column: "ApplicationName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CT_Buildings_FKUserUpdater",
                table: "CT_Buildings",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "UX_CT_Buildings_FKSite_Name",
                table: "CT_Buildings",
                columns: new[] { "FKSite", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CT_Carousel_FKApplication",
                table: "CT_Carousel",
                column: "FKApplication");

            migrationBuilder.CreateIndex(
                name: "IX_CT_Carousel_FKUserUpdater",
                table: "CT_Carousel",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "IX_CT_Categories_FKUserUpdater",
                table: "CT_Categories",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "UX_CT_Categories_Name",
                table: "CT_Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CT_CategoryApplications_FKCategory",
                table: "CT_CategoryApplications",
                column: "FKCategory");

            migrationBuilder.CreateIndex(
                name: "IX_CT_CategoryApplications_FKUserUpdater",
                table: "CT_CategoryApplications",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "UX_CT_CategoryApplications_FKApplication_FKCategory",
                table: "CT_CategoryApplications",
                columns: new[] { "FKApplication", "FKCategory" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CT_Groups_FKUserUpdater",
                table: "CT_Groups",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "UX_CT_Groups_Name",
                table: "CT_Groups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CT_Maintenance_FkApplication",
                table: "CT_Maintenance",
                column: "FkApplication");

            migrationBuilder.CreateIndex(
                name: "IX_CT_Maintenance_FKUserUpdater",
                table: "CT_Maintenance",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "UX_CT_Roles_Name",
                table: "CT_Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CT_Settings_SettingName",
                table: "CT_Settings",
                column: "SettingName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CT_Sites_Name",
                table: "CT_Sites",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SC_Users_FKRole",
                table: "SC_Users",
                column: "FKRole");

            migrationBuilder.CreateIndex(
                name: "UX_SC_Users_UserName",
                table: "SC_Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SY_ApplicationGroups_FKGroup",
                table: "SY_ApplicationGroups",
                column: "FKGroup");

            migrationBuilder.CreateIndex(
                name: "IX_SY_ApplicationGroups_FKUserUpdater",
                table: "SY_ApplicationGroups",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "UX_SY_ApplicationGroups_FKApplication_FKGroup",
                table: "SY_ApplicationGroups",
                columns: new[] { "FKApplication", "FKGroup" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SY_ApplicationLocation_FKBuilding",
                table: "SY_ApplicationLocation",
                column: "FKBuilding");

            migrationBuilder.CreateIndex(
                name: "IX_SY_ApplicationLocation_FKUserUpdater",
                table: "SY_ApplicationLocation",
                column: "FKUserUpdater");

            migrationBuilder.CreateIndex(
                name: "UX_SY_ApplicationLocation_FKApplication_FKBuilding",
                table: "SY_ApplicationLocation",
                columns: new[] { "FKApplication", "FKBuilding" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CT_Advices");

            migrationBuilder.DropTable(
                name: "CT_Carousel");

            migrationBuilder.DropTable(
                name: "CT_CategoryApplications");

            migrationBuilder.DropTable(
                name: "CT_Maintenance");

            migrationBuilder.DropTable(
                name: "CT_Settings");

            migrationBuilder.DropTable(
                name: "SY_ApplicationGroups");

            migrationBuilder.DropTable(
                name: "SY_ApplicationLocation");

            migrationBuilder.DropTable(
                name: "CT_Categories");

            migrationBuilder.DropTable(
                name: "CT_Groups");

            migrationBuilder.DropTable(
                name: "CT_Applications");

            migrationBuilder.DropTable(
                name: "CT_Buildings");

            migrationBuilder.DropTable(
                name: "CT_Sites");

            migrationBuilder.DropTable(
                name: "SC_Users");

            migrationBuilder.DropTable(
                name: "CT_Roles");
        }
    }
}
