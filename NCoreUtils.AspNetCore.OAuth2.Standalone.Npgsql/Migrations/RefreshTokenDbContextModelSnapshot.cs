﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NCoreUtils.OAuth2.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NCoreUtils.AspNetCore.OAuth2.Migrations
{
    [DbContext(typeof(RefreshTokenDbContext))]
    partial class RefreshTokenDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.RefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Email")
                        .HasColumnName("email")
                        .HasColumnType("character varying(768)")
                        .HasMaxLength(768)
                        .IsUnicode(true);

                    b.Property<long>("ExpiresAt")
                        .HasColumnName("expires_at")
                        .HasColumnType("bigint");

                    b.Property<long>("IssuedAt")
                        .HasColumnName("issued_at")
                        .HasColumnType("bigint");

                    b.Property<string>("Issuer")
                        .IsRequired()
                        .HasColumnName("issuer")
                        .HasColumnType("character varying(320)")
                        .HasMaxLength(320)
                        .IsUnicode(true);

                    b.Property<string>("Scopes")
                        .IsRequired()
                        .HasColumnName("scopes")
                        .HasColumnType("text")
                        .IsUnicode(true);

                    b.Property<string>("Sub")
                        .IsRequired()
                        .HasColumnName("sub")
                        .HasColumnType("character varying(128)")
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnName("username")
                        .HasColumnType("character varying(320)")
                        .HasMaxLength(320)
                        .IsUnicode(true);

                    b.HasKey("Id");

                    b.HasIndex("Sub", "IssuedAt");

                    b.ToTable("refresh_token");
                });
#pragma warning restore 612, 618
        }
    }
}
