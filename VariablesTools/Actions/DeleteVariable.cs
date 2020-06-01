using Astral.Logic.Classes.Map;
using VariableTools.Editors;
using VariableTools.Expressions;
using MyNW.Classes;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using VariableTools.Classes;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using static VariableTools.Classes.VariableCollection;
using MyNW.Internals;

namespace VariableTools.Actions
{
    [Serializable]
    public class DeleteVariable : Astral.Quester.Classes.Action
    {
#region Опции команды
        [Description("Идентификатор (имя) переменной.\n" +
                     "В имени переменной допускается использовние букв, цифр и символа \'_\'\n" +
                     "The Name of the {Variable}.")]
        //[Category("Variable options")]
        [DisplayName("Variable")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Editor(typeof(VariableSelectUiEditor), typeof(UITypeEditor))]
        public VariableKey Key
        {
            get => key;
            set
            {
                if (!ReferenceEquals(key, value))
                {
                    key = value;
                    lable = string.Empty;
                }
            }
        }
        private VariableKey key = new VariableKey();
        #endregion

        public override bool NeedToRun => true;

        public override ActionResult Run()
        {
            if (Key.IsValid && VariableTools.Variables.ContainsKey(Key))
            {
                if (VariableTools.Variables.Remove(Key))
                {
#if DEBUG
                    if (VariableTools.DebugMessage)
                        Astral.Logger.WriteLine(Astral.Logger.LogType.Debug, string.Concat(nameof(VariableTools), "::", GetType().Name, '[', ActionID,
                            "]: Character '", (EntityManager.LocalPlayer?.InternalName is null || string.IsNullOrEmpty(EntityManager.LocalPlayer.InternalName)) ? "Offline" : EntityManager.LocalPlayer.InternalName,
                            "' delete the variable  '", Key.ToString(), '\''));
#endif
                    return ActionResult.Completed;
                }
                else
                {
#if DEBUG
                    if (VariableTools.DebugMessage)
                        Astral.Logger.WriteLine(Astral.Logger.LogType.Debug, string.Concat(nameof(VariableTools), "::", GetType().Name, '[', ActionID,
                            "]: Character '", (EntityManager.LocalPlayer?.InternalName is null || string.IsNullOrEmpty(EntityManager.LocalPlayer.InternalName)) ? "Offline" : EntityManager.LocalPlayer.InternalName,
                            "' failed to delete the variable  '", Key.ToString(), '\''));
#endif
                    return ActionResult.Fail;
                }
            }
#if DEBUG
            if (VariableTools.DebugMessage)
                Astral.Logger.WriteLine(Astral.Logger.LogType.Debug, string.Concat(nameof(VariableTools), "::", GetType().Name, '[', ActionID,
                    "]: variable  '", Key.ToString(), "' doesn't found in the collection."));
#endif
            return ActionResult.Skip;
        }

        #region Интерфейс Quester.Action
        public override string ActionLabel => string.Empty;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(lable))
            {
                if (Key != null)
                    lable = $"{GetType().Name}: {Key.ToString()}";
                else lable = GetType().Name;
            }
            return lable;
        }
        private string lable = string.Empty;


        public override string InternalDisplayName => string.Empty;
        public override bool UseHotSpots => false;
        protected override bool IntenalConditions => key.IsValid;
        protected override Vector3 InternalDestination => new Vector3();
        protected override ActionValidity InternalValidity
        {
            get
            {
                if (Key.IsValid)
                    return new ActionValidity();
                else return new ActionValidity("The name of the {Variable} contains forbidden symbols or equals to Function name");
            }
        }
        public override void GatherInfos() { }
        public override void InternalReset() { }
        public override void OnMapDraw(GraphicsNW graph) { } 
        #endregion
    }
}