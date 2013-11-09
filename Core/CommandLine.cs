using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public class CommandLine
    {
        public enum Error
        {
            Success = 0,
            UnknownOption,
            NoValue,
            BadValue,
        }

        public static Error parseCommandLine(Object commandLineOptions)
        {
            var ArgsType = commandLineOptions.GetType();

            String[] Arguments = Environment.GetCommandLineArgs();
            for (int i = 1; i < Arguments.Length; )
            {
                String ArgName = Arguments[i];
                ++i;

                var Property = ArgsType.GetProperty(ArgName);
                if (Property == null)
                    return Error.UnknownOption;
                if (Property.PropertyType == typeof(Boolean))
                    Property.SetValue(commandLineOptions, true, null);
                else
                {
                    if (i >= Arguments.Length) return Error.NoValue;
                    try
                    {
                        Property.SetValue(commandLineOptions, System.Convert.ChangeType(Arguments[i], Property.PropertyType), null);
                    }
                    catch (Exception e)
                    {
                        return Error.BadValue;
                    }
                    ++i;
                }
            }

            return Error.Success;
        }
    }
}
