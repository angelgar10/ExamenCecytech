using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExamenCecytech.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Competencias",
                columns: table => new
                {
                    CompetenciaId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(maxLength: 50, nullable: false),
                    TiempoParaResolver = table.Column<int>(nullable: false),
                    LecturaPrevia = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competencias", x => x.CompetenciaId);
                });

            migrationBuilder.CreateTable(
                name: "Especialidades",
                columns: table => new
                {
                    EspecialidadId = table.Column<string>(maxLength: 12, nullable: false),
                    Nombre = table.Column<string>(maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.EspecialidadId);
                });

            migrationBuilder.CreateTable(
                name: "Planteles",
                columns: table => new
                {
                    PlantelId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClavePlantel = table.Column<string>(maxLength: 3, nullable: false),
                    Nombre = table.Column<string>(maxLength: 100, nullable: false),
                    ClaveCentroTrabajo = table.Column<string>(maxLength: 10, nullable: true),
                    ClaveSIIACE = table.Column<string>(maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planteles", x => x.PlantelId);
                    table.UniqueConstraint("AK_Planteles_ClavePlantel", x => x.ClavePlantel);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
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
                name: "Preguntas",
                columns: table => new
                {
                    PreguntaId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompetenciaId = table.Column<int>(nullable: false),
                    NumeroPregunta = table.Column<int>(nullable: false),
                    Orden1 = table.Column<int>(nullable: false),
                    Orden2 = table.Column<int>(nullable: false),
                    Texto = table.Column<string>(nullable: false),
                    LecturaPrevia = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preguntas", x => x.PreguntaId);
                    table.UniqueConstraint("AK_Preguntas_NumeroPregunta", x => x.NumeroPregunta);
                    table.UniqueConstraint("AK_Preguntas_CompetenciaId_PreguntaId", x => new { x.CompetenciaId, x.PreguntaId });
                    table.ForeignKey(
                        name: "FK_Preguntas_Competencias_CompetenciaId",
                        column: x => x.CompetenciaId,
                        principalTable: "Competencias",
                        principalColumn: "CompetenciaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    GrupoId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClavePlantel = table.Column<string>(maxLength: 3, nullable: false),
                    Nombre = table.Column<string>(maxLength: 40, nullable: false),
                    ClaveSIIACE = table.Column<string>(maxLength: 5, nullable: true),
                    Turno = table.Column<string>(maxLength: 1, nullable: true),
                    Semestre = table.Column<string>(maxLength: 1, nullable: true),
                    FechaExamen = table.Column<DateTime>(nullable: false),
                    EvaluacionHabilitada = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.GrupoId);
                    table.ForeignKey(
                        name: "FK_Grupos_Planteles_ClavePlantel",
                        column: x => x.ClavePlantel,
                        principalTable: "Planteles",
                        principalColumn: "ClavePlantel",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasPreguntas",
                columns: table => new
                {
                    RespuestaPreguntaId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompetenciaId = table.Column<int>(nullable: false),
                    PreguntaId = table.Column<int>(nullable: false),
                    Orden1 = table.Column<int>(nullable: false),
                    Orden2 = table.Column<int>(nullable: false),
                    Texto = table.Column<string>(nullable: false),
                    ClaveCOSDAC = table.Column<string>(maxLength: 1, nullable: false),
                    Valor = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasPreguntas", x => x.RespuestaPreguntaId);
                    table.UniqueConstraint("AK_RespuestasPreguntas_CompetenciaId_PreguntaId_ClaveCOSDAC", x => new { x.CompetenciaId, x.PreguntaId, x.ClaveCOSDAC });
                    table.UniqueConstraint("AK_RespuestasPreguntas_CompetenciaId_PreguntaId_RespuestaPreguntaId", x => new { x.CompetenciaId, x.PreguntaId, x.RespuestaPreguntaId });
                    table.ForeignKey(
                        name: "FK_RespuestasPreguntas_Preguntas_CompetenciaId_PreguntaId",
                        columns: x => new { x.CompetenciaId, x.PreguntaId },
                        principalTable: "Preguntas",
                        principalColumns: new[] { "CompetenciaId", "PreguntaId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Paterno = table.Column<string>(maxLength: 40, nullable: false),
                    Materno = table.Column<string>(maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(maxLength: 40, nullable: false),
                    Estatus = table.Column<string>(maxLength: 2, nullable: true),
                    Ficha = table.Column<string>(maxLength: 18, nullable: false),
                    GrupoId = table.Column<int>(nullable: true),
                    Genero = table.Column<string>(maxLength: 1, nullable: false),
                    Edad = table.Column<decimal>(nullable: false),
                    PromedioSecundaria = table.Column<decimal>(nullable: false),
                    NombreSecundaria = table.Column<string>(maxLength: 100, nullable: false),
                    TipoSecundaria = table.Column<string>(maxLength: 50, nullable: false),
                    DescripcionOtraSecundaria = table.Column<string>(maxLength: 50, nullable: false),
                    TipoSostenimientoSecundaria = table.Column<string>(maxLength: 50, nullable: false),
                    PlainPass = table.Column<string>(maxLength: 50, nullable: false),
                    EspecialidadId = table.Column<string>(maxLength: 12, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.UniqueConstraint("AK_AspNetUsers_Ficha", x => x.Ficha);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Especialidades_EspecialidadId",
                        column: x => x.EspecialidadId,
                        principalTable: "Especialidades",
                        principalColumn: "EspecialidadId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "GrupoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
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
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
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
                    UserId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false)
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
                    UserId = table.Column<int>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
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
                name: "RespuestasEvaluaciones",
                columns: table => new
                {
                    RespuestaEvaluacionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AspiranteId = table.Column<int>(nullable: false),
                    CompetenciaId = table.Column<int>(nullable: false),
                    PreguntaId = table.Column<int>(nullable: false),
                    RespuestaPreguntaId = table.Column<int>(nullable: false),
                    FechaCreacion = table.Column<DateTime>(nullable: false),
                    UsuarioCreacion = table.Column<string>(maxLength: 255, nullable: false),
                    FechaModificacion = table.Column<DateTime>(nullable: false),
                    UsuarioModificacion = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasEvaluaciones", x => x.RespuestaEvaluacionId);
                    table.UniqueConstraint("AK_RespuestasEvaluaciones_AspiranteId_CompetenciaId_PreguntaId", x => new { x.AspiranteId, x.CompetenciaId, x.PreguntaId });
                    table.ForeignKey(
                        name: "FK_RespuestasEvaluaciones_AspNetUsers_AspiranteId",
                        column: x => x.AspiranteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespuestasEvaluaciones_Preguntas_CompetenciaId_PreguntaId",
                        columns: x => new { x.CompetenciaId, x.PreguntaId },
                        principalTable: "Preguntas",
                        principalColumns: new[] { "CompetenciaId", "PreguntaId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespuestasEvaluaciones_RespuestasPreguntas_CompetenciaId_PreguntaId_RespuestaPreguntaId",
                        columns: x => new { x.CompetenciaId, x.PreguntaId, x.RespuestaPreguntaId },
                        principalTable: "RespuestasPreguntas",
                        principalColumns: new[] { "CompetenciaId", "PreguntaId", "RespuestaPreguntaId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosPlantel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ClavePlantel = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosPlantel", x => new { x.Id, x.ClavePlantel });
                    table.ForeignKey(
                        name: "FK_UsuariosPlantel_Planteles_ClavePlantel",
                        column: x => x.ClavePlantel,
                        principalTable: "Planteles",
                        principalColumn: "ClavePlantel",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosPlantel_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EspecialidadId",
                table: "AspNetUsers",
                column: "EspecialidadId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GrupoId",
                table: "AspNetUsers",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Competencias_Nombre",
                table: "Competencias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_ClavePlantel",
                table: "Grupos",
                column: "ClavePlantel");

            migrationBuilder.CreateIndex(
                name: "IX_Planteles_ClaveCentroTrabajo",
                table: "Planteles",
                column: "ClaveCentroTrabajo",
                unique: true,
                filter: "[ClaveCentroTrabajo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Planteles_ClaveSIIACE",
                table: "Planteles",
                column: "ClaveSIIACE",
                unique: true,
                filter: "[ClaveSIIACE] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasEvaluaciones_CompetenciaId_PreguntaId_RespuestaPreguntaId",
                table: "RespuestasEvaluaciones",
                columns: new[] { "CompetenciaId", "PreguntaId", "RespuestaPreguntaId" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPlantel_ClavePlantel",
                table: "UsuariosPlantel",
                column: "ClavePlantel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "RespuestasEvaluaciones");

            migrationBuilder.DropTable(
                name: "UsuariosPlantel");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "RespuestasPreguntas");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Preguntas");

            migrationBuilder.DropTable(
                name: "Especialidades");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropTable(
                name: "Competencias");

            migrationBuilder.DropTable(
                name: "Planteles");
        }
    }
}
