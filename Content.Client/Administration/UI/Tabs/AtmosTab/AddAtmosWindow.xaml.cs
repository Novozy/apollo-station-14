using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Robust.Client.AutoGenerated;
using Robust.Client.Console;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;

namespace Content.Client.Administration.UI.Tabs.AtmosTab
{
    [GenerateTypedNameReferences]
    [UsedImplicitly]
    public sealed partial class AddAtmosWindow : DefaultWindow
    {
        private IEnumerable<IMapGrid>? _data;

        protected override void EnteredTree()
        {
            _data = IoCManager.Resolve<IMapManager>().GetAllGrids().Where(g => (int) g.GridEntityId != 0);
            foreach (var grid in _data)
            {
                var player = IoCManager.Resolve<IPlayerManager>().LocalPlayer?.ControlledEntity;
                var playerGrid = IoCManager.Resolve<IEntityManager>().GetComponentOrNull<TransformComponent>(player)?.GridEntityId;
                GridOptions.AddItem($"{grid.GridEntityId} {(playerGrid == grid.GridEntityId ? " (Current)" : "")}");
            }

            GridOptions.OnItemSelected += eventArgs => GridOptions.SelectId(eventArgs.Id);
            SubmitButton.OnPressed += SubmitButtonOnOnPressed;
        }

        private void SubmitButtonOnOnPressed(BaseButton.ButtonEventArgs obj)
        {
            if (_data == null)
                return;
            var dataList = _data.ToList();
            var selectedGrid = dataList[GridOptions.SelectedId].GridEntityId;
            IoCManager.Resolve<IClientConsoleHost>().ExecuteCommand($"addatmos {selectedGrid}");
        }
    }
}