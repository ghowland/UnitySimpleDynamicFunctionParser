using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParseSimpleCode
{
    public enum ParseUtilSymbolType
    {
        None,
        Comma,
        ParenOpen,
        ParenClose,
        QuoteDouble,
    }

    [System.Serializable]
    public class ParseUtilSymbolItem
    {
        public ParseUtilSymbolType Type;
        public string Value;

        public ParseUtilSymbolItem(ParseUtilSymbolType type, string value)
        {
            Type = type;
            Value = value;
        }
    }

    [System.Serializable]
    public class ParseUtilSymbolic
    {
        public List<ParseUtilSymbolItem> Items;

        public ParseUtilSymbolic(string text)
        {
            Items = new List<ParseUtilSymbolItem>();

            ParseUtilSymbolItem curItem = null;
            for (int pos = 0; pos < text.Length; pos++)
            {
                switch (text[pos])
                {
                    case ',':
                        Items.Add(new ParseUtilSymbolItem(ParseUtilSymbolType.Comma, null));
                        curItem = null;
                        break;
                    case '(':
                        Items.Add(new ParseUtilSymbolItem(ParseUtilSymbolType.ParenOpen, null));
                        curItem = null;
                        break;
                    case ')':
                        Items.Add(new ParseUtilSymbolItem(ParseUtilSymbolType.ParenClose, null));
                        curItem = null;
                        break;
                    case '"':
                        Items.Add(new ParseUtilSymbolItem(ParseUtilSymbolType.QuoteDouble, null));
                        curItem = null;
                        break;
                    default:
                        if (curItem == null)
                        {
                            curItem = new ParseUtilSymbolItem(ParseUtilSymbolType.None, "");
                            Items.Add(curItem);
                        }

                        curItem.Value += text[pos];
                        break;
                }
            }
        }

        public ParseUtilCommand ParseCommand()
        {
            ParseUtilCommand topCommand = null;

            List<ParseUtilCommand> commandStack = new List<ParseUtilCommand>();

            bool insideQuote = false;
            string insideQuoteStr = "";

            // Work forwards
            for (int index = 0; index < Items.Count; index++)
            {
                // If this is a open, add it to our list
                if (Items[index].Type == ParseUtilSymbolType.ParenOpen && !insideQuote)
                {
                    // If we just had a last value in current stack, remove it
                    if (commandStack.Count > 0 && commandStack[commandStack.Count - 1].Args.Count > 0)
                    {
                        commandStack[commandStack.Count - 1].Args.RemoveAt(commandStack[commandStack.Count - 1].Args.Count - 1);
                    }

                    // The argument was the last value
                    ParseUtilCommand newCommand = new ParseUtilCommand(Items[index - 1].Value, null);

                    // Ensure we have a top command, which is our first one
                    if (topCommand == null) topCommand = newCommand;

                    // Add this new command as a new argument to the last command in the stack, as we go deeper
                    if (commandStack.Count > 0)
                    {
                        var newCommandArg = new ParseUtilCommandArg(newCommand);
                        commandStack[commandStack.Count - 1].Args.Add(newCommandArg);
                    }

                    // Add ourselves to the stack
                    commandStack.Add(newCommand);
                }
                // Else, if this is an close
                else if (Items[index].Type == ParseUtilSymbolType.ParenClose && !insideQuote)
                {
                    // Remove this command from the stack
                    commandStack.RemoveAt(commandStack.Count - 1);
                }
                // Else, normal argument, just add it
                else
                {
                    if (commandStack.Count > 0)
                    {
                        switch (Items[index].Type)
                        {
                            case ParseUtilSymbolType.Comma:
                                // If we are quoted, then add the command into the quote, which is desired
                                if (insideQuote)
                                {
                                    insideQuoteStr += ",";
                                }
                                break;
                            case ParseUtilSymbolType.ParenOpen:
                                insideQuoteStr += "(";
                                break;
                            case ParseUtilSymbolType.ParenClose:
                                insideQuoteStr += ")";
                                break;
                            case ParseUtilSymbolType.QuoteDouble:
                                // If we werent quoting, we are now
                                if (!insideQuote)
                                {
                                    insideQuote = true;
                                    insideQuoteStr = "";
                                }
                                // Else, we were quoting, so wrap it up and add the quote
                                else
                                {
                                    insideQuote = false;
                                    var newArgInside = new ParseUtilCommandArg(insideQuoteStr);
                                    commandStack[commandStack.Count - 1].Args.Add(newArgInside);
                                }
                                break;
                            case ParseUtilSymbolType.None:
                                // If quoting, append to quote
                                if (insideQuote)
                                {
                                    insideQuoteStr += Items[index].Value;
                                }
                                // Else, add stand alone value
                                else
                                {
                                    // Ensure we dont add empty space as a value.  This could happen in front of a Double quote, for example
                                    if (Items[index].Value.Trim() != "")
                                    {
                                        var newArg = new ParseUtilCommandArg(Items[index].Value.Trim());
                                        commandStack[commandStack.Count - 1].Args.Add(newArg);
                                    }
                                }
                                break;
                        }

                    }
                }

            }

            return topCommand;
        }

        public string Print()
        {
            string output = "";

            foreach (var item in Items)
            {
                output += $"{item.Type}:{item.Value}  ";
            }

            return output;
        }
    }

    [System.Serializable]
    public class ParseUtilCommandArg
    {
        [Tooltip("If not null, this argument is actually an embedded command")]
        public ParseUtilCommand Command;

        [Tooltip("Else, its just a string")]
        public string Value;

        public ParseUtilCommandArg(string line)
        {
            Command = null;
            Value = line;
        }

        public ParseUtilCommandArg(ParseUtilCommand command)
        {
            Command = command;
            Value = null;
        }
    }

    [System.Serializable]
    public class ParseUtilCommand
    {
        public string Name;
        public List<ParseUtilCommandArg> Args;

        public ParseUtilCommand(string name, List<ParseUtilCommandArg> args)
        {
            Name = name;

            if (args != null) Args = args;
            else Args = new List<ParseUtilCommandArg>();
        }
    }
}

