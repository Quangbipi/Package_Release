using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Quang.DesignPattern.Tests
{
    // EventBus<T> API uses Raise, not Publish. Adjusted from the plan's assumed API
    // during Ph5 execution. Each test uses its own event struct to avoid bleeding
    // into other tests via EventBus<T>'s static per-type binding set (tests unsubscribe
    // explicitly; EventBusRegistry.ClearAllBindings would wipe every other bus too).
    public class EventBusTests
    {
        private struct SubscribeReceivesRaiseEvent : global::Quang.DesignPattern.IEvent { public int Value; }
        private struct UnsubscribeStopsReceivingEvent : global::Quang.DesignPattern.IEvent { }
        private struct NoSubscribersEvent : global::Quang.DesignPattern.IEvent { }
        private struct TwoSubscribersEvent : global::Quang.DesignPattern.IEvent { }
        private struct SubscriberExceptionEvent : global::Quang.DesignPattern.IEvent { }
        private struct UnsubscribeDuringRaiseEvent : global::Quang.DesignPattern.IEvent { }
        private struct SubscribeDuringRaiseEvent : global::Quang.DesignPattern.IEvent { }
        private struct ClearAllBindingsEvent : global::Quang.DesignPattern.IEvent { }

        [Test]
        public void Subscribe_ReceivesRaise()
        {
            int received = -1;
            var binding = new global::Quang.DesignPattern.EventBinding<SubscribeReceivesRaiseEvent>(e => received = e.Value);
            global::Quang.DesignPattern.EventBus<SubscribeReceivesRaiseEvent>.Subscribe(binding);

            global::Quang.DesignPattern.EventBus<SubscribeReceivesRaiseEvent>.Raise(new SubscribeReceivesRaiseEvent { Value = 42 });

            global::Quang.DesignPattern.EventBus<SubscribeReceivesRaiseEvent>.Unsubscribe(binding);
            Assert.AreEqual(42, received);
        }

        [Test]
        public void Unsubscribe_StopsReceiving()
        {
            int callCount = 0;
            var binding = new global::Quang.DesignPattern.EventBinding<UnsubscribeStopsReceivingEvent>(() => callCount++);
            global::Quang.DesignPattern.EventBus<UnsubscribeStopsReceivingEvent>.Subscribe(binding);
            global::Quang.DesignPattern.EventBus<UnsubscribeStopsReceivingEvent>.Unsubscribe(binding);

            global::Quang.DesignPattern.EventBus<UnsubscribeStopsReceivingEvent>.Raise(new UnsubscribeStopsReceivingEvent());

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Raise_NoSubscribers_NoThrow()
        {
            Assert.DoesNotThrow(() =>
                global::Quang.DesignPattern.EventBus<NoSubscribersEvent>.Raise(new NoSubscribersEvent()));
        }

        [Test]
        public void TwoSubscribers_BothReceive()
        {
            int callsA = 0, callsB = 0;
            var bindingA = new global::Quang.DesignPattern.EventBinding<TwoSubscribersEvent>(() => callsA++);
            var bindingB = new global::Quang.DesignPattern.EventBinding<TwoSubscribersEvent>(() => callsB++);
            global::Quang.DesignPattern.EventBus<TwoSubscribersEvent>.Subscribe(bindingA);
            global::Quang.DesignPattern.EventBus<TwoSubscribersEvent>.Subscribe(bindingB);

            global::Quang.DesignPattern.EventBus<TwoSubscribersEvent>.Raise(new TwoSubscribersEvent());

            global::Quang.DesignPattern.EventBus<TwoSubscribersEvent>.Unsubscribe(bindingA);
            global::Quang.DesignPattern.EventBus<TwoSubscribersEvent>.Unsubscribe(bindingB);
            Assert.AreEqual(1, callsA);
            Assert.AreEqual(1, callsB);
        }

        [Test]
        public void SubscriberException_DoesNotBlockOthers()
        {
            var throwing = new global::Quang.DesignPattern.EventBinding<SubscriberExceptionEvent>(() => throw new System.Exception("boom"));
            bool secondCalled = false;
            var second = new global::Quang.DesignPattern.EventBinding<SubscriberExceptionEvent>(() => secondCalled = true);
            global::Quang.DesignPattern.EventBus<SubscriberExceptionEvent>.Subscribe(throwing);
            global::Quang.DesignPattern.EventBus<SubscriberExceptionEvent>.Subscribe(second);

            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("boom"));
            Assert.DoesNotThrow(() =>
                global::Quang.DesignPattern.EventBus<SubscriberExceptionEvent>.Raise(new SubscriberExceptionEvent()));

            global::Quang.DesignPattern.EventBus<SubscriberExceptionEvent>.Unsubscribe(throwing);
            global::Quang.DesignPattern.EventBus<SubscriberExceptionEvent>.Unsubscribe(second);
            Assert.IsTrue(secondCalled);
        }
        // BUG-0217 regression. Raise used to enumerate the live binding set, so a handler
        // that mutated its own bus threw out of MoveNext - which the per-iteration catch
        // inside Raise cannot intercept - silently skipping every binding after it.
        [Test]
        public void UnsubscribeDuringRaise_DoesNotThrowAndLaterBindingsStillFire()
        {
            int laterCalls = 0;
            global::Quang.DesignPattern.EventBinding<UnsubscribeDuringRaiseEvent> selfRemoving = null;
            selfRemoving = new global::Quang.DesignPattern.EventBinding<UnsubscribeDuringRaiseEvent>(
                () => global::Quang.DesignPattern.EventBus<UnsubscribeDuringRaiseEvent>.Unsubscribe(selfRemoving));
            var later = new global::Quang.DesignPattern.EventBinding<UnsubscribeDuringRaiseEvent>(() => laterCalls++);

            global::Quang.DesignPattern.EventBus<UnsubscribeDuringRaiseEvent>.Subscribe(selfRemoving);
            global::Quang.DesignPattern.EventBus<UnsubscribeDuringRaiseEvent>.Subscribe(later);

            Assert.DoesNotThrow(() =>
                global::Quang.DesignPattern.EventBus<UnsubscribeDuringRaiseEvent>.Raise(new UnsubscribeDuringRaiseEvent()));
            Assert.AreEqual(1, laterCalls, "a self-unsubscribing handler must not stop later bindings from firing");

            // The mutation still took effect: the removed binding is gone for the next raise.
            global::Quang.DesignPattern.EventBus<UnsubscribeDuringRaiseEvent>.Raise(new UnsubscribeDuringRaiseEvent());
            Assert.AreEqual(2, laterCalls);

            global::Quang.DesignPattern.EventBus<UnsubscribeDuringRaiseEvent>.Unsubscribe(later);
        }

        // Pins the chosen semantic: Raise snapshots its binding set, so a handler added
        // mid-raise fires on the NEXT raise, never on the in-flight event.
        [Test]
        public void SubscribeDuringRaise_DoesNotReceiveTheInFlightEvent()
        {
            int lateCalls = 0;
            var late = new global::Quang.DesignPattern.EventBinding<SubscribeDuringRaiseEvent>(() => lateCalls++);
            var adder = new global::Quang.DesignPattern.EventBinding<SubscribeDuringRaiseEvent>(
                () => global::Quang.DesignPattern.EventBus<SubscribeDuringRaiseEvent>.Subscribe(late));

            global::Quang.DesignPattern.EventBus<SubscribeDuringRaiseEvent>.Subscribe(adder);

            Assert.DoesNotThrow(() =>
                global::Quang.DesignPattern.EventBus<SubscribeDuringRaiseEvent>.Raise(new SubscribeDuringRaiseEvent()));
            Assert.AreEqual(0, lateCalls, "a binding added mid-raise must not receive the in-flight event");

            global::Quang.DesignPattern.EventBus<SubscribeDuringRaiseEvent>.Unsubscribe(adder);
            global::Quang.DesignPattern.EventBus<SubscribeDuringRaiseEvent>.Raise(new SubscribeDuringRaiseEvent());
            Assert.AreEqual(1, lateCalls, "it must receive the next raise");

            global::Quang.DesignPattern.EventBus<SubscribeDuringRaiseEvent>.Unsubscribe(late);
        }

        // Domain reload off: a binding left behind by a stopped play session must not fire in
        // the next one. ClearAllBindings is what SubsystemRegistration runs on play start.
        [Test]
        public void ClearAllBindings_DropsSubscriberLeftFromPreviousSession()
        {
            int callCount = 0;
            var stale = new global::Quang.DesignPattern.EventBinding<ClearAllBindingsEvent>(() => callCount++);
            global::Quang.DesignPattern.EventBus<ClearAllBindingsEvent>.Subscribe(stale);

            global::Quang.DesignPattern.EventBusRegistry.ClearAllBindings();
            global::Quang.DesignPattern.EventBus<ClearAllBindingsEvent>.Raise(new ClearAllBindingsEvent());

            Assert.AreEqual(0, callCount);
        }
    }
}