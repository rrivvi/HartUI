using HartUI.Components;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

internal static class FormsRegisteredByRounder
{
    private sealed class RegisteredForm
    {
        public Form TargetForm;
        public cuiFormRounder Rounder;
    }

    private static readonly List<RegisteredForm> registeredFormList = new List<RegisteredForm>();

    private static void CleanupInvalidForms()
    {
        registeredFormList.RemoveAll(f => f.TargetForm == null || f.TargetForm.IsDisposed);
    }

    public static void RemoveByForm(Form form)
    {
        registeredFormList.RemoveAll(f => f.TargetForm == form);
    }

    public static bool AddByForm(Form formToAdd, cuiFormRounder rounderToAdd)
    {
        CleanupInvalidForms();

        if (GetRounderByForm(formToAdd) != null)
        {
            return false;
        }

        registeredFormList.Add(new RegisteredForm
        {
            TargetForm = formToAdd,
            Rounder = rounderToAdd
        });

        return true;
    }

    public static cuiFormRounder GetRounderByForm(Form formSelector)
    {
        CleanupInvalidForms();

        return registeredFormList.FirstOrDefault(f => f.TargetForm == formSelector)?.Rounder;
    }
}