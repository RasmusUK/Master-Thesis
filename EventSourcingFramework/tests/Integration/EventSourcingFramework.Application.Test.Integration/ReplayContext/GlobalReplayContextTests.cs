using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Test.Utilities;
using EventSourcingFramework.Test.Utilities.Models;

namespace EventSourcingFramework.Application.Test.Integration.ReplayContext;

[Collection("Integration")]
public class GlobalReplayContextTests : MongoIntegrationTestBase
{
    private readonly IGlobalReplayContext context;

    public GlobalReplayContextTests(IMongoDbService mongoDbService, IGlobalReplayContext context)
        : base(mongoDbService, context)
    {
        this.context = context;
    }

    [Fact]
    public void StartReplay_SetsIsReplayingAndReplayMode()
    {
        // Act
        context.StartReplay(ReplayMode.Sandbox);

        // Assert
        Assert.True(context.IsReplaying);
        Assert.Equal(ReplayMode.Sandbox, context.ReplayMode);
    }

    [Fact]
    public void StartReplay_WhenAlreadyReplaying_Throws()
    {
        // Arrange
        context.StartReplay();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => context.StartReplay());
        Assert.Equal("Replay is already in progress.", ex.Message);
    }

    [Fact]
    public void StopReplay_SetsIsReplayingToFalse()
    {
        // Arrange
        context.StartReplay();

        // Act
        context.StopReplay();

        // Assert
        Assert.False(context.IsReplaying);
    }

    [Fact]
    public void StopReplay_WhenNotReplaying_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => context.StopReplay());
        Assert.Equal("Replay is not in progress.", ex.Message);
    }

    [Fact]
    public void GetEvents_WhenNotReplaying_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => context.GetEvents());
        Assert.Equal("Replay is not in progress.", ex.Message);
    }

    [Fact]
    public void AddEvent_WhenNotReplaying_Throws()
    {
        var evt = new TestEvent();
        var ex = Assert.Throws<InvalidOperationException>(() => context.AddEvent(evt));
        Assert.Equal("Replay is not in progress.", ex.Message);
    }

    [Fact]
    public void AddEvent_WhenReplayingInSandbox_AddsEvent()
    {
        // Arrange
        context.StartReplay(ReplayMode.Sandbox);
        var evt = new TestEvent();

        // Act
        context.AddEvent(evt);
        var events = context.GetEvents();

        // Assert
        Assert.Single(events);
        Assert.Contains(evt, events);
    }

    [Fact]
    public void IsLoading_CanBeSetAndRetrieved()
    {
        // Act
        context.IsLoading = true;

        // Assert
        Assert.True(context.IsLoading);

        context.IsLoading = false;
        Assert.False(context.IsLoading);
    }
}