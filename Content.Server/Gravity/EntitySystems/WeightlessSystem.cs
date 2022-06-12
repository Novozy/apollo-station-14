using Content.Shared.Alert;
using Content.Shared.GameTicking;
using Content.Shared.Gravity;
using Content.Shared.Movement.Components;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server.Gravity.EntitySystems
{
    [UsedImplicitly]
    public sealed class WeightlessSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly AlertsSystem _alertsSystem = default!;

        private readonly Dictionary<EntityUid, List<AlertsComponent>> _alerts = new();

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundRestartCleanupEvent>(Reset);
            SubscribeLocalEvent<GravityChangedMessage>(GravityChanged);
            SubscribeLocalEvent<AlertsComponent, EntParentChangedMessage>(EntParentChanged);
            SubscribeLocalEvent<AlertsComponent, AlertSyncEvent>(HandleAlertSyncEvent);
        }

        public void Reset(RoundRestartCleanupEvent ev)
        {
            _alerts.Clear();
        }

        public void AddAlert(AlertsComponent status)
        {
            var xform = Transform(status.Owner);
            var alerts = _alerts.GetOrNew(xform.GridEntityId);

            alerts.Add(status);

            if (_mapManager.TryGetGrid(xform.GridEntityId, out var grid))
            {
                if (EntityManager.GetComponent<GravityComponent>(grid.GridEntityId).Enabled)
                {
                    RemoveWeightless(status.Owner);
                }
                else
                {
                    AddWeightless(status.Owner);
                }
            }
        }

        public void RemoveAlert(AlertsComponent status)
        {
            var grid = EntityManager.GetComponent<TransformComponent>(status.Owner).GridEntityId;
            if (!_alerts.TryGetValue(grid, out var statuses))
            {
                return;
            }

            statuses.Remove(status);
        }

        private void GravityChanged(GravityChangedMessage ev)
        {
            if (!_alerts.TryGetValue(ev.ChangedGridIndex, out var statuses))
            {
                return;
            }

            if (ev.HasGravity)
            {
                foreach (var status in statuses)
                {
                    RemoveWeightless(status.Owner);
                }
            }
            else
            {
                foreach (var status in statuses)
                {
                    AddWeightless(status.Owner);
                }
            }
        }

        private void AddWeightless(EntityUid euid)
        {
            _alertsSystem.ShowAlert(euid, AlertType.Weightless);
        }

        private void RemoveWeightless(EntityUid euid)
        {
            _alertsSystem.ClearAlert(euid, AlertType.Weightless);
        }

        private void EntParentChanged(EntityUid uid, AlertsComponent status, ref EntParentChangedMessage ev)
        {
            // First, update the `_alerts` dictionary
            if (ev.OldParent is {Valid: true} old &&
                EntityManager.TryGetComponent(old, out IMapGridComponent? mapGrid))
            {
                var oldGrid = mapGrid.Owner;

                if (_alerts.TryGetValue(oldGrid, out var oldStatuses))
                {
                    oldStatuses.Remove(status);
                }
            }

            var newGrid = ev.Transform.GridEntityId;
            var newStatuses = _alerts.GetOrNew(newGrid);

            newStatuses.Add(status);

            // then update the actual alert. The alert is only removed if either the player is on a grid with gravity,
            // or if they ignore gravity-based movement altogether.
            // TODO: update this when planets and the like are added.
            // TODO: update alert when the ignore-gravity component is added or removed.
            if (_mapManager.TryGetGrid(newGrid, out var grid)
                && TryComp(grid.GridEntityId, out GravityComponent? gravity)
                && gravity.Enabled)
                RemoveWeightless(status.Owner);
            else if (!HasComp<MovementIgnoreGravityComponent>(uid))
                AddWeightless(status.Owner);
        }

        private void HandleAlertSyncEvent(EntityUid uid, AlertsComponent component, AlertSyncEvent args)
        {
            switch (component.LifeStage)
            {
                case ComponentLifeStage.Starting:
                    AddAlert(component);
                    break;
                case ComponentLifeStage.Removing:
                    RemoveAlert(component);
                    break;
            }
        }
    }
}