using EFArchiver.Attributes;
using EFArchiver.Helpers;

namespace EFArchiver.Tests.Helpers
{
    public class PartitionMetadataHelperTests
    {
        private class DummyEntity
        {
            public Guid Id { get; set; }
            [PartitionKey(ThresholdDays = 90)]
            public DateTime CreatedAt { get; set; }
        }

        [Fact]
        public void GetPartitionKeyProperty_ShouldReturn_CreatedAtProperty()
        {
            var prop = PartitionMetadataHelper.GetPartitionKeyProperty<DummyEntity>();
            Assert.NotNull(prop);
            Assert.Equal(nameof(DummyEntity.CreatedAt), prop!.Name);
        }

        [Fact]
        public void GetPartitionKeySettings_ShouldReturn_CorrectThresholdDays()
        {
            var settings = PartitionMetadataHelper.GetPartitionKeySettings<DummyEntity>();
            Assert.NotNull(settings);
            Assert.Equal(90, settings!.ThresholdDays);
            Assert.Null(settings.EqualTo);
        }
    }
}
