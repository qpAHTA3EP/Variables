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
    public enum SaveState
    {
        /// <summary>
        /// Не сохранять
        /// </summary>
        False,
        /// <summary>
        /// Сохранять
        /// </summary>
        True,
        /// <summary>
        /// Не менять значение флага сохранения
        /// </summary>
        NotChange
    }

    [Serializable]
    public class SetVariable : Astral.Quester.Classes.Action
    {
#region Опции команды
        [Description("Идентификатор (имя) переменной.\n" +
                     "В имени переменной допускается использовние букв, цифр и символа \'_\'\n" +
                     "The Name of the {Variable}.")]
        [Category("Variable options")]
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

        [Browsable(false)]
        public string Variable
        {
            get => key.Name;
            set
            {
                if (key.Name != value)
                {
                    if (Parser.CorrectForbiddenName(value, out string corrected))
                    {
                        // Имя переменной некорректно
                        // Запрашиваем замену
                        if (XtraMessageBox.Show(/*Form.ActiveForm, */
                                                $"Задано недопустимое имя переменно '{value}'!\n" +
                                                $"Хотите его исправить на '{corrected}'?\n" +
                                                $"The name '{value}' is incorrect! \n" +
                                                $"Whould you like to change it to '{corrected}'?",
                                                "Warring", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        {
                            // Пользователь не согласился заменить некорректное имя переменной
                            //Save = false;
                            //AccountScope = AccountScopeType.Global;
                            //ProfileScope = ProfileScopeType.Common;
                            //StoredValue = 0;
                            //variableContainer = null;

                            key.Name = value;
                            //variableNameOk = false;
                        }
                        else
                        {
                            // Пользователь согласился заменить имя переменной на корректное
                            key.Name = corrected;
                            //variableNameOk = true;
                        }
                    }
                    else
                    {
                        key.Name = value;
                        //variableNameOk = true;
                    }
                }
            }
        }

        [Category("Variable options")]
        [XmlIgnore]
        [Editor(typeof(StoreVariableEditor), typeof(UITypeEditor))]
        [Description("Нажми на кнопку '...' чтобы сохранить переменную в коллекцию\n" +
                     "Press button '...' to store the variable to the collection")]
        public string VariableToCollection { get; } = "Нажми на кнопку '...' =>";


        [Editor(typeof(EquationUiEditor), typeof(UITypeEditor))]
        [Description("Выражение, результат которого присваивается переменной {Variable}.\n" +
                     "An expression whose result is assigned to the {Variable}.")]
        [Category("What will assign to the Variable")]
        public NumberExpression Equation
        {
            get => equation;
            set
            {
                if (value.Text != equation.Text)
                {
                    equation = value;
                    lable = string.Empty;
                }
            }
        }
        private NumberExpression equation = new NumberExpression();

        [Description("Флаг, указывающий на необходимость сохранения переменной в файл.\n" +
             "Flag that orders to save the variable to a file.")]
        [Category("Variable options")]
#if false
        public bool Save { get; set; } = false; 
#else
        public SaveState Save { get; set; } = SaveState.NotChange;
#endif
        #endregion

        public override bool NeedToRun => true;

        public override ActionResult Run()
        {
            if (Key.IsValid && equation.IsValid)
            {
                if (equation.Calcucate(out double result))
                {
#if DEBUG
                    if (VariableTools.DebugMessage)
                        Astral.Logger.WriteLine(Astral.Logger.LogType.Debug, $"{nameof(VariableTools)}::{GetType().Name}[{ActionID}]: Equation '{equation.Text}' result is {result}.");
#endif

                    // Реализация через VariableCollection
                    if (VariableTools.Variables.TryGetValue(out VariableContainer variable, Key))
                    {
                        variable.Value = result;
                        switch (Save)
                        {
                            case SaveState.True:
                                variable.Save = true;
                                break;
                            case SaveState.False:
                                variable.Save = true;
                                break;
                        }
#if DEBUG
                        if (VariableTools.DebugMessage)
                            Astral.Logger.WriteLine(Astral.Logger.LogType.Debug, string.Concat(nameof(VariableTools), "::", GetType().Name, '[', ActionID,
                                "]: Character '", (EntityManager.LocalPlayer?.InternalName is null || string.IsNullOrEmpty(EntityManager.LocalPlayer.InternalName)) ? "Offline" : EntityManager.LocalPlayer.InternalName,
                                "' assign the value '", variable.Value, "' to the Variable {", variable.Name, "}[", variable.AccountScope, ", ", variable.ProfileScope, 
                                "] (as the result of the Equation = ", equation.Text,"). Save = ", Save));
#endif
                        return ActionResult.Completed;
                    }
                    else
                    {
                        variable = VariableTools.Variables.Add(result, Key.Name, Key.AccountScope, Key.ProfileScope);
                        if (variable != null)
                        {
                            switch (Save)
                            {
                                case SaveState.True:
                                    variable.Save = true;
                                    break;
                                case SaveState.False:
                                    variable.Save = true;
                                    break;
                            }
#if DEBUG
                            if (VariableTools.DebugMessage)
                                Astral.Logger.WriteLine(Astral.Logger.LogType.Debug, string.Concat(nameof(VariableTools), "::", GetType().Name, '[', ActionID,
                                    "]: Character '", (EntityManager.LocalPlayer?.InternalName is null || string.IsNullOrEmpty(EntityManager.LocalPlayer.InternalName)) ? "Offline" : EntityManager.LocalPlayer.InternalName,
                                    "' initialize the Variable {", variable.ToString(), "} with the value '", variable.Value, 
                                    "' (as the result of the Equation = ", equation.Text, "). Save = ", Save));

#endif
                            return ActionResult.Completed;
                        }
                        else
                        {
#if DEBUG
                            if(VariableTools.DebugMessage)
                                Astral.Logger.WriteLine(Astral.Logger.LogType.Debug, string.Concat(nameof(VariableTools), "::", GetType().Name, '[', ActionID,
                                    "]: Character '", (EntityManager.LocalPlayer?.InternalName is null || string.IsNullOrEmpty(EntityManager.LocalPlayer.InternalName)) ? "Offline" : EntityManager.LocalPlayer.InternalName, 
                                    "' FAILED to initialize the Variable {", Key.ToString(), '}'));
#endif
                            return ActionResult.Fail;
                        }
                    }
                }
#if DEBUG
                else Astral.Logger.WriteLine(Astral.Logger.LogType.Debug, $"{nameof(VariableTools)}::{GetType().Name}[{ActionID}]: Equation '{equation.Text}' calculation FAILED.");
#endif
            }
            return ActionResult.Fail;
        }

        #region Интерфейс Quester.Action
        public override string ActionLabel => string.Empty;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(lable))
            {
                if (Key != null)
                {
                    if (equation != null || !string.IsNullOrEmpty(equation.Text))
                        lable = $"{GetType().Name}: {Key.ToString()} := {equation.Text}";
                    else lable = $"{GetType().Name}: {Key.ToString()}";
                }
                else lable = GetType().Name;
            }
            return lable;
        }
        private string lable = string.Empty;

        public override string InternalDisplayName => string.Empty;
        public override bool UseHotSpots => false;
        protected override bool IntenalConditions => key.IsValid && equation.IsValid;
        protected override Vector3 InternalDestination => new Vector3();
        protected override ActionValidity InternalValidity
        {
            get
            {
                if (equation.IsValid)
                {
                    if (Key != null && Key.IsValid)
                        return new ActionValidity();
                    else return new ActionValidity("The name of the {Variable} contains forbidden symbols or equals to Function name");
                }
                else
                {
                    if (Key != null && Key.IsValid)
                        return new ActionValidity("Equation is incorrect");
                    else return new ActionValidity("Equation is incorrect.\n" +
                        "The name of the {Variable} contains forbidden symbols or equals to Function name");
                }
            }
        }
        public override void GatherInfos() { }
        public override void InternalReset() { }
        public override void OnMapDraw(GraphicsNW graph) { } 
        #endregion
    }
}