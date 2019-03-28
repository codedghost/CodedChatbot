﻿// <auto-generated />
using System;
using CoreCodedChatbot.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CoreCodedChatbot.Database.Migrations
{
    [DbContext(typeof(ChatbotContext))]
    [Migration("20190327233437_AddingOtherInformationCommands")]
    partial class AddingOtherInformationCommands
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.InfoCommand", b =>
                {
                    b.Property<int>("InfoCommandId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("InfoCommandId");

                    b.Property<string>("InfoHelpText")
                        .IsRequired()
                        .HasColumnName("InfoHelpText");

                    b.Property<string>("InfoText")
                        .IsRequired()
                        .HasColumnName("InfoText");

                    b.HasKey("InfoCommandId");

                    b.ToTable("InfoCommands");
                });

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.InfoCommandKeyword", b =>
                {
                    b.Property<int>("InfoCommandId")
                        .HasColumnName("InfoCommandId");

                    b.Property<string>("InfoCommandKeywordText")
                        .HasColumnName("InfoCommandKeywordText");

                    b.HasKey("InfoCommandId", "InfoCommandKeywordText");

                    b.ToTable("InfoCommandKeywords");
                });

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.Setting", b =>
                {
                    b.Property<int>("SettingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("SettingId");

                    b.Property<string>("SettingName")
                        .IsRequired()
                        .HasColumnName("SettingName");

                    b.Property<string>("SettingValue")
                        .IsRequired()
                        .HasColumnName("SettingValue");

                    b.HasKey("SettingId");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.Song", b =>
                {
                    b.Property<int>("SongId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("SongId");

                    b.Property<string>("SongArtist")
                        .IsRequired()
                        .HasColumnName("SongArtist");

                    b.Property<string>("SongName")
                        .IsRequired()
                        .HasColumnName("SongName");

                    b.HasKey("SongId");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.SongGuessingRecord", b =>
                {
                    b.Property<int>("SongGuessingRecordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("SongGuessingRecordId");

                    b.Property<decimal>("FinalPercentage")
                        .HasColumnName("FinalPercentage");

                    b.Property<bool>("IsInProgress")
                        .HasColumnName("IsInProgress");

                    b.Property<string>("SongDetails")
                        .IsRequired()
                        .HasColumnName("SongDetails");

                    b.Property<bool>("UsersCanGuess")
                        .HasColumnName("UsersCanGuess");

                    b.HasKey("SongGuessingRecordId");

                    b.ToTable("SongGuessingRecord");
                });

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.SongPercentageGuess", b =>
                {
                    b.Property<int>("SongPercentageGuessId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("SongPercentageGuessId");

                    b.Property<decimal>("Guess")
                        .HasColumnName("Guess");

                    b.Property<int>("SongGuessingRecordId")
                        .HasColumnName("SongGuessingRecordId");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnName("Username");

                    b.HasKey("SongPercentageGuessId");

                    b.HasIndex("SongGuessingRecordId");

                    b.ToTable("SongPercentageGuess");
                });

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.SongRequest", b =>
                {
                    b.Property<int>("SongRequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("SongRequestId");

                    b.Property<bool>("InDrive");

                    b.Property<bool>("Played");

                    b.Property<string>("RequestText");

                    b.Property<DateTime>("RequestTime");

                    b.Property<string>("RequestUsername")
                        .IsRequired()
                        .HasColumnName("RequestUsername")
                        .HasMaxLength(255);

                    b.Property<int>("SongId")
                        .HasColumnName("SongId");

                    b.Property<DateTime?>("VipRequestTime");

                    b.HasKey("SongRequestId");

                    b.ToTable("SongRequests");
                });

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.User", b =>
                {
                    b.Property<string>("Username")
                        .HasColumnName("Username");

                    b.Property<int>("DonationOrBitsVipRequests")
                        .HasColumnName("DonationOrBitsVipRequests");

                    b.Property<int>("FollowVipRequest")
                        .HasColumnName("FollowVipRequest");

                    b.Property<int>("ModGivenVipRequests")
                        .HasColumnName("ModGivenVipRequests");

                    b.Property<int>("ReceivedGiftVipRequests")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ReceivedGiftVipRequests")
                        .HasDefaultValue(0);

                    b.Property<int>("SentGiftVipRequests")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("SendgiftVipRequests")
                        .HasDefaultValue(0);

                    b.Property<int>("SubVipRequests")
                        .HasColumnName("SubVipRequests");

                    b.Property<DateTime>("TimeLastInChat")
                        .HasColumnName("TimeLastInChat");

                    b.Property<int>("TokenBytes");

                    b.Property<int>("TokenVipRequests");

                    b.Property<int>("TotalBitsDropped")
                        .HasColumnName("TotalBitsDropped");

                    b.Property<int>("TotalDonated")
                        .HasColumnName("TotalDonated");

                    b.Property<int>("UsedVipRequests")
                        .HasColumnName("UsedVipRequests");

                    b.HasKey("Username");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.InfoCommandKeyword", b =>
                {
                    b.HasOne("CoreCodedChatbot.Database.Context.Models.InfoCommand", "InfoCommand")
                        .WithMany("InfoCommandKeywords")
                        .HasForeignKey("InfoCommandId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("CoreCodedChatbot.Database.Context.Models.SongPercentageGuess", b =>
                {
                    b.HasOne("CoreCodedChatbot.Database.Context.Models.SongGuessingRecord", "SongGuessingRecord")
                        .WithMany("SongPercentageGuesses")
                        .HasForeignKey("SongGuessingRecordId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
