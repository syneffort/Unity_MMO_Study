﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.DB;

namespace Server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20210125133514_item")]
    partial class item
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Server.DB.AccountDb", b =>
                {
                    b.Property<int>("AccountDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("AccountName")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("AccountDbId");

                    b.HasIndex("AccountName")
                        .IsUnique()
                        .HasFilter("[AccountName] IS NOT NULL");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Server.DB.ItemDb", b =>
                {
                    b.Property<int>("ItemDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<int?>("OwnerDbId")
                        .HasColumnType("int");

                    b.Property<int>("Slot")
                        .HasColumnType("int");

                    b.Property<string>("TemplateId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ItemDbId");

                    b.HasIndex("OwnerDbId");

                    b.ToTable("Item");
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.Property<int>("PlayerDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("AccountDbId")
                        .HasColumnType("int");

                    b.Property<int>("Attack")
                        .HasColumnType("int");

                    b.Property<int>("Hp")
                        .HasColumnType("int");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<int>("MaxHp")
                        .HasColumnType("int");

                    b.Property<string>("PlayerName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<float>("Speed")
                        .HasColumnType("real");

                    b.Property<int>("TotalExp")
                        .HasColumnType("int");

                    b.HasKey("PlayerDbId");

                    b.HasIndex("AccountDbId");

                    b.HasIndex("PlayerName")
                        .IsUnique()
                        .HasFilter("[PlayerName] IS NOT NULL");

                    b.ToTable("Player");
                });

            modelBuilder.Entity("Server.DB.ItemDb", b =>
                {
                    b.HasOne("Server.DB.PlayerDb", "Owner")
                        .WithMany("Items")
                        .HasForeignKey("OwnerDbId");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.HasOne("Server.DB.AccountDb", "Account")
                        .WithMany("Players")
                        .HasForeignKey("AccountDbId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Server.DB.AccountDb", b =>
                {
                    b.Navigation("Players");
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
