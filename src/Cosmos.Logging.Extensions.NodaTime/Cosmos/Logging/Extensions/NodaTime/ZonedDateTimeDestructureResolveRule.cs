using Cosmos.Logging.Extensions.NodaTime.Core;
using NodaTime;
using NodaTime.Text;

namespace Cosmos.Logging.Extensions.NodaTime {
    internal sealed class ZonedDateTimeDestructureResolveRule : NodaTimeDestructureResolveRule<ZonedDateTime> {
        protected override IPattern<ZonedDateTime> Pattern { get; }

        public ZonedDateTimeDestructureResolveRule(IDateTimeZoneProvider provider) : base(CreateIsoValidator(x => x.Calendar)) {
            Pattern = ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFFo<G> z", provider);
        }
    }
}