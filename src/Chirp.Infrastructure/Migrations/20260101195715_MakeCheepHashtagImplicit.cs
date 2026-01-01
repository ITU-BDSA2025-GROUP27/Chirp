using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chirp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeCheepHashtagImplicit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheepHashtags_Cheeps_CheepId",
                table: "CheepHashtags");

            migrationBuilder.DropForeignKey(
                name: "FK_CheepHashtags_Hashtags_HashtagId",
                table: "CheepHashtags");

            migrationBuilder.RenameColumn(
                name: "HashtagId",
                table: "CheepHashtags",
                newName: "HashtagsHashtagId");

            migrationBuilder.RenameColumn(
                name: "CheepId",
                table: "CheepHashtags",
                newName: "CheepsCheepId");

            migrationBuilder.RenameIndex(
                name: "IX_CheepHashtags_HashtagId",
                table: "CheepHashtags",
                newName: "IX_CheepHashtags_HashtagsHashtagId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheepHashtags_Cheeps_CheepsCheepId",
                table: "CheepHashtags",
                column: "CheepsCheepId",
                principalTable: "Cheeps",
                principalColumn: "CheepId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CheepHashtags_Hashtags_HashtagsHashtagId",
                table: "CheepHashtags",
                column: "HashtagsHashtagId",
                principalTable: "Hashtags",
                principalColumn: "HashtagId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheepHashtags_Cheeps_CheepsCheepId",
                table: "CheepHashtags");

            migrationBuilder.DropForeignKey(
                name: "FK_CheepHashtags_Hashtags_HashtagsHashtagId",
                table: "CheepHashtags");

            migrationBuilder.RenameColumn(
                name: "HashtagsHashtagId",
                table: "CheepHashtags",
                newName: "HashtagId");

            migrationBuilder.RenameColumn(
                name: "CheepsCheepId",
                table: "CheepHashtags",
                newName: "CheepId");

            migrationBuilder.RenameIndex(
                name: "IX_CheepHashtags_HashtagsHashtagId",
                table: "CheepHashtags",
                newName: "IX_CheepHashtags_HashtagId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheepHashtags_Cheeps_CheepId",
                table: "CheepHashtags",
                column: "CheepId",
                principalTable: "Cheeps",
                principalColumn: "CheepId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CheepHashtags_Hashtags_HashtagId",
                table: "CheepHashtags",
                column: "HashtagId",
                principalTable: "Hashtags",
                principalColumn: "HashtagId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
