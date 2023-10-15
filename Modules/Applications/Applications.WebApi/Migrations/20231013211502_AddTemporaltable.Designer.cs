﻿// <auto-generated />
using System;
using Applications.Application.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Applications.WebApi.Migrations
{
    [DbContext(typeof(CreditApplicationDbContext))]
    [Migration("20231013211502_AddTemporaltable")]
    partial class AddTemporaltable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Applications.Application.Domain.Application.CreditApplication", b =>
                {
                    b.Property<string>("ApplicationId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("CreditPeriodInMonths")
                        .HasColumnType("int");

                    b.Property<DateTime>("PeriodEnd")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2")
                        .HasColumnName("PeriodEnd");

                    b.Property<DateTime>("PeriodStart")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2")
                        .HasColumnName("PeriodStart");

                    b.HasKey("ApplicationId");

                    b.ToTable("CreditApplication", (string)null);

                    b.ToTable(tb => tb.IsTemporal(ttb =>
                            {
                                ttb.UseHistoryTable("CreditApplicationHistory");
                                ttb
                                    .HasPeriodStart("PeriodStart")
                                    .HasColumnName("PeriodStart");
                                ttb
                                    .HasPeriodEnd("PeriodEnd")
                                    .HasColumnName("PeriodEnd");
                            }));
                });

            modelBuilder.Entity("Applications.Application.Domain.Application.CreditApplication", b =>
                {
                    b.OwnsOne("Applications.Application.Domain.Application.CustomerPersonalData", "CustomerPersonalData", b1 =>
                        {
                            b1.Property<string>("CreditApplicationApplicationId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<string>("FirstName")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("LastName")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Pesel")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("CreditApplicationApplicationId");

                            b1.ToTable("CreditApplication");

                            b1.ToJson("CustomerPersonalData");

                            b1.WithOwner()
                                .HasForeignKey("CreditApplicationApplicationId");
                        });

                    b.OwnsOne("Applications.Application.Domain.Application.Declaration", "Declaration", b1 =>
                        {
                            b1.Property<string>("CreditApplicationApplicationId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<decimal>("AverageNetMonthlyIncome")
                                .HasColumnType("decimal(18,2)");

                            b1.HasKey("CreditApplicationApplicationId");

                            b1.ToTable("CreditApplication");

                            b1.ToJson("Declaration");

                            b1.WithOwner()
                                .HasForeignKey("CreditApplicationApplicationId");
                        });

                    b.OwnsMany("Applications.Application.Domain.Application.State", "States", b1 =>
                        {
                            b1.Property<string>("CreditApplicationApplicationId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            b1.Property<DateTimeOffset?>("ContractSigningDate")
                                .HasColumnType("datetimeoffset");

                            b1.Property<DateTimeOffset>("Date")
                                .HasColumnType("datetimeoffset");

                            b1.Property<string>("Decision")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Level")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("CreditApplicationApplicationId", "Id");

                            b1.ToTable("CreditApplication");

                            b1.ToJson("States");

                            b1.WithOwner()
                                .HasForeignKey("CreditApplicationApplicationId");
                        });

                    b.Navigation("CustomerPersonalData")
                        .IsRequired();

                    b.Navigation("Declaration")
                        .IsRequired();

                    b.Navigation("States");
                });
#pragma warning restore 612, 618
        }
    }
}
