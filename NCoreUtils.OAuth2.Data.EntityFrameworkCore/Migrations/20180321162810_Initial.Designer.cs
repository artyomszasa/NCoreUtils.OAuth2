﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using NCoreUtils.Data;
using NCoreUtils.OAuth2.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace NCoreUtils.OAuth2.Data.EntityFrameworkCore.Migrations
{
    [DbContext(typeof(OAuth2DbContext))]
    [Migration("20180321162810_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.AuthorizationCode", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<long>("ExpiresAt");

                    b.Property<long>("IssuedAt");

                    b.Property<string>("RedirectUri")
                        .IsRequired()
                        .IsUnicode(false);

                    b.Property<string>("Scopes")
                        .IsRequired()
                        .IsUnicode(false);

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AuthorizationCode");
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.ClientApplication", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .HasMaxLength(8000)
                        .IsUnicode(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .IsUnicode(true);

                    b.HasKey("Id");

                    b.ToTable("ClientApplication");
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.Domain", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ClientApplicationId");

                    b.Property<string>("DomainName")
                        .HasMaxLength(4000)
                        .IsUnicode(true);

                    b.HasKey("Id");

                    b.HasIndex("ClientApplicationId");

                    b.HasIndex("DomainName")
                        .IsUnique();

                    b.ToTable("Domain");
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ClientApplicationId");

                    b.Property<string>("Description")
                        .HasMaxLength(4000)
                        .IsUnicode(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.HasIndex("ClientApplicationId");

                    b.HasIndex("Name", "ClientApplicationId");

                    b.ToTable("Permission");
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.RefreshToken", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ExpiresAt");

                    b.Property<long>("IssuedAt");

                    b.Property<long?>("LastUsed");

                    b.Property<string>("Scopes")
                        .IsRequired()
                        .IsUnicode(false);

                    b.Property<int>("State");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("State", "UserId", "IssuedAt", "ExpiresAt", "Scopes");

                    b.ToTable("RefreshToken");
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ClientApplicationId");

                    b.Property<long>("Created");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(500)
                        .IsUnicode(true);

                    b.Property<string>("FamilyName")
                        .IsRequired()
                        .HasMaxLength(250)
                        .IsUnicode(true);

                    b.Property<string>("GivenName")
                        .HasMaxLength(2000)
                        .IsUnicode(true);

                    b.Property<string>("HonorificPrefix")
                        .HasMaxLength(60)
                        .IsUnicode(true);

                    b.Property<string>("MiddleName")
                        .HasMaxLength(2000)
                        .IsUnicode(true);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<int>("State");

                    b.Property<long>("Updated");

                    b.HasKey("Id");

                    b.HasIndex("ClientApplicationId");

                    b.HasIndex("Created");

                    b.HasIndex("Updated");

                    b.HasIndex("Email", "ClientApplicationId")
                        .IsUnique();

                    b.ToTable("User");
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.UserPermission", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<int>("PermissionId");

                    b.HasKey("UserId", "PermissionId");

                    b.HasIndex("PermissionId");

                    b.ToTable("UserPermission");
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.AuthorizationCode", b =>
                {
                    b.HasOne("NCoreUtils.OAuth2.Data.User", "User")
                        .WithMany("AuthorizationCodes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.Domain", b =>
                {
                    b.HasOne("NCoreUtils.OAuth2.Data.ClientApplication", "ClientApplication")
                        .WithMany("Domains")
                        .HasForeignKey("ClientApplicationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.Permission", b =>
                {
                    b.HasOne("NCoreUtils.OAuth2.Data.ClientApplication", "ClientApplication")
                        .WithMany("Permissions")
                        .HasForeignKey("ClientApplicationId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.RefreshToken", b =>
                {
                    b.HasOne("NCoreUtils.OAuth2.Data.User", "User")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.User", b =>
                {
                    b.HasOne("NCoreUtils.OAuth2.Data.ClientApplication", "ClientApplication")
                        .WithMany("Users")
                        .HasForeignKey("ClientApplicationId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("NCoreUtils.OAuth2.Data.UserPermission", b =>
                {
                    b.HasOne("NCoreUtils.OAuth2.Data.Permission", "Permission")
                        .WithMany("Users")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("NCoreUtils.OAuth2.Data.User", "User")
                        .WithMany("Permissions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
