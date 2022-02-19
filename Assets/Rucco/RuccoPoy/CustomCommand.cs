using UnityEngine;

namespace RuccoPoyLang
{
    /// <summary>
    /// Parent class for every CustomCommand.
    /// Inherit from this class if you want to create a CustomCommand!
    /// </summary>

    public class CustomCommand : MonoBehaviour
    {
        public string Key { get; }

        public string Description { get; }

        /// <summary>
        /// The base constructor for any CustomCommand.
        /// Every time you create new CustomCommand,
        /// remember to also give parameters to the base constructor!
        /// </summary>
        
        /// <param name="key">
        /// The keyword that is used to access the CustomCommand.
        /// It has to be in CAPS since otherwise the command can't be accessed!
        /// </param>
        
        /// <param name="description">
        /// The description that is shown in the doc_custom_commands print.
        /// Every string in the string array equals one line of text.
        /// Do not add your own newlines there, or otherwise the description
        /// will look ugly.
        /// </param>
        
        public CustomCommand(string key, string[] description)
        {
            this.Key = key;
            this.Description = "\n\n";

            foreach (string line in description)
                this.Description += "  " + line + '\n';
        }

        /// <summary>
        /// The main method for any CustomCommand.
        /// Everything you want your CustomCommand to do, must be called here.
        /// </summary>

        /// <param name="parameters">
        /// These are the parameters that is given to the CustomCommand in the script.
        /// Remeber to use DemandParam for every parameter you know you want to have.
        /// Otherwise array error will occur!
        /// </param>

        /// <returns>
        /// Returns the value as a string.
        /// Remember to use Return methods for numbers, and ReturnEmpty
        /// if you don't want to return anything.
        /// </returns>

        public virtual string Command(string[] parameters) => ReturnEmpty();

        /// <summary>
        /// Use this to add description to your demanded parameter
        /// </summary>
        
        /// <param name="paramName">Parameter name</param>
        /// <param name="paramIndex">Index of the parameter starting from 1</param>

        protected static string DescDemandedParam(string paramName, int paramIndex)
            => $"*P{paramIndex} = {paramName}";

        /// <summary>
        /// Use this to add description to your optional parameter
        /// </summary>
        
        /// <param name="paramName">Parameter name</param>
        /// <param name="paramIndex">Index of the parameter starting from 1</param>
        
        protected static string DescOptionalParam(string paramName, int paramIndex)
            => $" P{paramIndex} = {paramName}";

        /// <summary>
        /// Use this to tell that there is no parameters
        /// </summary>

        protected static string DescNoParameters()
            => $" Doesn't take parameters";

        protected void DemandParam(string[] parameters, int index, string errMsg)
        {
            if (parameters[index] != null && parameters[index] != "")
                return;
            Error.Throw(ErrorOrigin.CustomCommand, errMsg);
        }

        protected void DemandParamAmt(string[] parameters, int amt)
        {
            if(parameters.Length < amt)
                Error.Throw
                (
                    ErrorOrigin.CustomCommand,
                    $"Custom command {Key} demands {amt} parameter, but {parameters.Length} was given"
                );
        }

        protected bool IsParamAmt(string[] parameters, int amt)
        {
            if(parameters.Length < amt)
                return false;
            return true;
        }

        /// <summary>
        /// If you want to return int value, use this.
        /// </summary>

        protected string Return(int num) => num + string.Empty;

        /// <summary>
        /// If you want to return float value, use this.
        /// </summary>

        protected string Return(float num) => num + string.Empty;

        /// <summary>
        /// If you don't want to return anything, use this.
        /// </summary>

        protected string ReturnEmpty() => string.Empty;
        
        /// <summary>
        /// Used for casting a string to float.
        /// Gives Rucco console error message if given value can't be casted.
        /// Created to be used when casting parameters to floats.
        /// </summary>
        
        /// <param name="str">String value</param>
        /// <param name="errMsg">Error message to be shown in the Rucco console</param>
        
        /// <returns>Float number</returns>
        
        protected float Float(string str, string errMsg)
        {
            float result = 0.0f;

            if (float.TryParse(str, out result))
                return result;
            else
                Error.Throw(ErrorOrigin.CustomCommand, errMsg);

            return 0.0f;
        }
    }
}