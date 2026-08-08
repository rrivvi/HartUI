using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

#if NET472
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
#else
using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Actions;
using IDesignerHost = System.ComponentModel.Design.IDesignerHost;
using IComponentChangeService = System.ComponentModel.Design.IComponentChangeService;
using ISelectionService = System.ComponentModel.Design.ISelectionService;
using DesignerTransaction = System.ComponentModel.Design.DesignerTransaction;
using SelectionTypes = System.ComponentModel.Design.SelectionTypes;
#endif

namespace HartUI.Misc.Internal
{
    internal class DesignerIntegration
    {
        // shared between controls and components
        internal static readonly DesignerIntegration SharedDesigner = new DesignerIntegration();

        private List<(Type, string, object)> _clipboard = new List<(Type, string, object)>();
        internal int CopiedCount => _clipboard.Count;
        internal Type CopiedFromType { get; private set; }

        private static readonly HashSet<string> excludedPropertyNames = new HashSet<string> {
            "Checked",
            "Content",
            "Image",
            "CheckButton",
            "TargetControl",
            "TargetForm",
            "Multiselect",
            "BoxAmount",
            "MinValue",
            "Value",
            "MaxValue",
            "Tasks",
            "TasksProgress",
            "SmallChange",
            "LargeChange",
            "StarCount",
            "Rating",
            "Pages",
            "Description",
            "ProgressValue",
            "Multiline",
            "DataPoints",
            "CustomXAxis",
            "DialogResult",
            "Group",
            "MaximumValue",
            "MinimumValue",
            "Items",
            "SelectedItem",
            "SelectedIndex",
            "TargetLocation",
            "Filter",
            "NormalContent",
            "UploadContent",
            "HoverContent",
            "OnlyDigit",
            "ScrollOffset",
            "SelectedTab",
            "NumericValue",
            "PlaceholderText"
        };

        internal static bool IsDisposed(Component component)
        {
            return component is Control control && control.IsDisposed;
        }

        private bool PassesChecks(PropertyDescriptor controlProperty, Type controlType)
        {
            return
                controlProperty.Category != null &&
                controlProperty.Category.Contains("HartUI") &&
                controlProperty.IsBrowsable &&
                controlType.IsVisible &&
                false == controlProperty.IsReadOnly &&
                false == excludedPropertyNames.Contains(controlProperty.Name);
        }

        internal void SetClipboard(Component targetControl)
        {
            if (targetControl == null || IsDisposed(targetControl))
            {
                return;
            }

            _clipboard.Clear();
            CopiedFromType = null;
            Type controlType = targetControl.GetType();

            foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(targetControl))
            {
                if (PassesChecks(desc, controlType))
                {
                    object value;
                    try
                    {
                        value = desc.GetValue(targetControl);
                    }
                    catch
                    {
                        continue;
                    }
                    _clipboard.Add((controlType, desc.Name, value));
                }
            }

            CopiedFromType = _clipboard.Count > 0 ? controlType : null;
        }

        internal void PasteClipboard(Component targetControl)
        {
            if (targetControl == null || IsDisposed(targetControl) || _clipboard.Count == 0)
            {
                return;
            }

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(targetControl);

            // already validated by SetClipboard so skip checks here
            foreach (var (sourceType, propertyName, value) in _clipboard)
            {
                PropertyDescriptor desc = properties.Find(propertyName, false);
                if (desc == null || desc.IsReadOnly)
                {
                    continue;
                }

                try
                {
                    desc.SetValue(targetControl, value);
                }
                catch
                {
                    // skip properties that throw on SetValue
                }
            }
        }

        internal class HartControlDesigner : ControlDesigner
        {
            private DesignerActionListCollection _actionLists;

            public override DesignerActionListCollection ActionLists
            {
                get
                {
                    if (!(Component is Control control) || control.IsDisposed)
                    {
                        return new DesignerActionListCollection();
                    }

                    if (_actionLists == null)
                    {
                        _actionLists = new DesignerActionListCollection
                        {
                            new HartActionList(control, SharedDesigner)
                        };
                    }
                    return _actionLists;
                }
            }
        }

        internal class HartComponentDesigner : ComponentDesigner
        {
            private DesignerActionListCollection _actionLists;

            public override DesignerActionListCollection ActionLists
            {
                get
                {
                    if (!(Component is Component component) || IsDisposed(component))
                    {
                        return new DesignerActionListCollection();
                    }

                    if (_actionLists == null)
                    {
                        _actionLists = new DesignerActionListCollection
                        {
                            new HartActionList(component, SharedDesigner)
                        };
                    }
                    return _actionLists;
                }
            }
        }

        internal class HartActionList : DesignerActionList
        {
            private readonly Component targetControl;
            private readonly DesignerIntegration designerIntegration;

            public HartActionList(Component control, DesignerIntegration integration) : base(control)
            {
                targetControl = control;
                designerIntegration = integration;
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                string appearanceOrSettings = (targetControl is Control) ? "Appearance" : "Settings";

                var items = new DesignerActionItemCollection
                {
                    new DesignerActionMethodItem(this, nameof(CopySettings), $"Copy {appearanceOrSettings}", true)
                };

                if (designerIntegration.CopiedCount > 0)
                {
                    items.Add(new DesignerActionMethodItem(this, nameof(PasteSettings), $"Paste {appearanceOrSettings} ({designerIntegration.CopiedCount} in clipboard from type {designerIntegration.CopiedFromType.Name})", true));
                }

                return items;
            }

            public void CopySettings()
            {
                if (targetControl == null || IsDisposed(targetControl))
                {
                    return;
                }

                try
                {
                    designerIntegration.SetClipboard(targetControl);
                }
                catch
                {
                    return;
                }

                RefreshPanel();
            }

            public void PasteSettings()
            {
                if (targetControl == null || IsDisposed(targetControl) || designerIntegration.CopiedCount == 0)
                {
                    return;
                }

                var host = (IDesignerHost)GetService(typeof(IDesignerHost));
                var changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));

                using (var t = host?.CreateTransaction("Paste Settings"))
                {
                    try
                    {
                        changeService?.OnComponentChanging(targetControl, null);
                        designerIntegration.PasteClipboard(targetControl);
                        changeService?.OnComponentChanged(targetControl, null, null, null);
                        t?.Commit();
                    }
                    catch
                    {
                        t?.Cancel();
                    }
                }

                TypeDescriptor.Refresh(targetControl);
            }

            private void RefreshPanel()
            {
                if (targetControl == null || IsDisposed(targetControl))
                {
                    return;
                }

                // floating smart tag
                var uiService = (DesignerActionUIService)GetService(typeof(DesignerActionUIService));
                uiService?.Refresh(Component);

                // properties window
                var selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                if (selectionService != null)
                {
                    selectionService.SetSelectedComponents(Array.Empty<object>(), SelectionTypes.Replace);
                    selectionService.SetSelectedComponents(new object[] { targetControl }, SelectionTypes.Replace);
                }
            }
        }
    }
}
