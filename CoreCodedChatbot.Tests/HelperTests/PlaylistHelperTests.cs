using System;

using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

using Xunit;

namespace CoreCodedChatbot.Tests.HelperTests
{
    public class PlaylistHelperTests
    {
        private IChatbotContextFactory CreateContextFactory()
        {
            var options = new DbContextOptionsBuilder<ChatbotContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var contextFactory = Substitute.For<IChatbotContextFactory>();
            contextFactory.Create().Returns(_ => new ChatbotContext(options));
            return contextFactory;
        }

        private PlaylistHelper CreatePlaylistHelper(IChatbotContextFactory contextFactory)
        {
            var configHelper = Substitute.For<IConfigHelper>();
            configHelper.GetConfig().Returns(new ConfigModel { ObsPlaylistPath = "obsplaylist.txt" });

            return new PlaylistHelper(contextFactory, configHelper);
        }

        [Fact]
        public void AddRequest_WhenPlaylistIsClosed_DeniesRegularRequest()
        {
            var contextFactory = this.CreateContextFactory();
            var context = contextFactory.Create();
            context.Settings.Add(new Setting { SettingName = "PlaylistStatus", SettingValue = "Closed" });
            context.SaveChanges();

            var result = this.CreatePlaylistHelper(contextFactory).AddRequest("test", "test", vipRequest: false);

            Assert.Equal(AddRequestResult.PlaylistClosed, result.Item1);
        }

        [Fact]
        public void AddRequest_WhenPlaylistIsClosed_AllowsVipRequest()
        {
            var contextFactory = this.CreateContextFactory();
            var context = contextFactory.Create();
            context.Settings.Add(new Setting { SettingName = "PlaylistStatus", SettingValue = "Closed" });
            context.SaveChanges();

            var result = this.CreatePlaylistHelper(contextFactory).AddRequest("test", "test", vipRequest: true);

            Assert.Equal(AddRequestResult.Success, result.Item1);
        }
    }
}
