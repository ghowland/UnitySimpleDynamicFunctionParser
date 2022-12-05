# Unity Simple Dynamic Function Parser

This is just a simple dynamic (from text) function parser.  So I can write embedded functions, with quoted strings, and I can then traverse inside Unity.

Full write up here:

https://allhailtemos.com/blog/dumbest-language-processor-devlog-20

Invoked like this:

var text = "Bark(\"Let's do it, yeah!\", 590099, Test5(1, Test6()), Test2(37, 94, \"Test, Bob()\", Test3()))";
var symbolic = new ParseUtilSymbolic(text);
var command = symbolic.ParseCommand(typeof(StoryCommandFunction));
