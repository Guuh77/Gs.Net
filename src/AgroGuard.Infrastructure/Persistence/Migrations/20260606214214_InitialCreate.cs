using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgroGuard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AG_CROPS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    ScientificName = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    IdealNdvi = table.Column<decimal>(type: "NUMBER(4,3)", nullable: false),
                    WaterDemandIndex = table.Column<decimal>(type: "NUMBER(4,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AG_CROPS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AG_USERS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(180)", maxLength: 180, nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Role = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AG_USERS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AG_FARMS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OwnerId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    City = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    State = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    Latitude = table.Column<decimal>(type: "NUMBER(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "NUMBER(9,6)", nullable: false),
                    TotalAreaHectares = table.Column<decimal>(type: "NUMBER(12,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AG_FARMS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AG_FARMS_AG_USERS_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AG_USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AG_FIELDS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FarmId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CropId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    AreaHectares = table.Column<decimal>(type: "NUMBER(12,2)", nullable: false),
                    Latitude = table.Column<decimal>(type: "NUMBER(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "NUMBER(9,6)", nullable: false),
                    SoilType = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    PlantedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ExpectedHarvestAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AG_FIELDS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AG_FIELDS_AG_CROPS_CropId",
                        column: x => x.CropId,
                        principalTable: "AG_CROPS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AG_FIELDS_AG_FARMS_FarmId",
                        column: x => x.FarmId,
                        principalTable: "AG_FARMS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AG_SATELLITE_READINGS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FieldId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    Source = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Ndvi = table.Column<decimal>(type: "NUMBER(4,3)", nullable: false),
                    SoilMoisturePercent = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    SurfaceTemperatureCelsius = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    RainfallMillimeters = table.Column<decimal>(type: "NUMBER(7,2)", nullable: false),
                    CloudCoveragePercent = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AG_SATELLITE_READINGS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AG_SATELLITE_READINGS_AG_FIELDS_FieldId",
                        column: x => x.FieldId,
                        principalTable: "AG_FIELDS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AG_RISK_ALERTS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FieldId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SatelliteReadingId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Level = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Score = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    Title = table.Column<string>(type: "NVARCHAR2(140)", maxLength: 140, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Recommendation = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AG_RISK_ALERTS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AG_RISK_ALERTS_AG_FIELDS_FieldId",
                        column: x => x.FieldId,
                        principalTable: "AG_FIELDS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AG_RISK_ALERTS_AG_SATELLITE_READINGS_SatelliteReadingId",
                        column: x => x.SatelliteReadingId,
                        principalTable: "AG_SATELLITE_READINGS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "AG_CROPS",
                columns: new[] { "Id", "IdealNdvi", "Name", "ScientificName", "WaterDemandIndex" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 0.82m, "Soybean", "Glycine max", 0.78m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 0.80m, "Corn", "Zea mays", 0.84m },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 0.74m, "Coffee", "Coffea arabica", 0.66m },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 0.86m, "Sugarcane", "Saccharum officinarum", 0.88m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AG_CROPS_Name",
                table: "AG_CROPS",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AG_FARMS_OwnerId_Name",
                table: "AG_FARMS",
                columns: new[] { "OwnerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AG_FIELDS_CropId",
                table: "AG_FIELDS",
                column: "CropId");

            migrationBuilder.CreateIndex(
                name: "IX_AG_FIELDS_FarmId_Name",
                table: "AG_FIELDS",
                columns: new[] { "FarmId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AG_RISK_ALERTS_FieldId_Status_Level",
                table: "AG_RISK_ALERTS",
                columns: new[] { "FieldId", "Status", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_AG_RISK_ALERTS_SatelliteReadingId",
                table: "AG_RISK_ALERTS",
                column: "SatelliteReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_AG_SATELLITE_READINGS_FieldId_CapturedAt",
                table: "AG_SATELLITE_READINGS",
                columns: new[] { "FieldId", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AG_USERS_Email",
                table: "AG_USERS",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AG_RISK_ALERTS");

            migrationBuilder.DropTable(
                name: "AG_SATELLITE_READINGS");

            migrationBuilder.DropTable(
                name: "AG_FIELDS");

            migrationBuilder.DropTable(
                name: "AG_CROPS");

            migrationBuilder.DropTable(
                name: "AG_FARMS");

            migrationBuilder.DropTable(
                name: "AG_USERS");
        }
    }
}
