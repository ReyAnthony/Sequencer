 Create sequences that run asynchronously on Monobehaviors
 
 ``` c#
Action<DataContext> setToTen = (ctx) => { ctx.i = 10; Debug.Log(ctx.i); };
Action<DataContext> increment = (ctx) => { ctx.i++; Debug.Log(ctx.i); };
Func<DataContext, bool> AboveTwenty = (ctx) => ctx.i > 20;
Action<DataContext> printDone = (ctx) => { Debug.Log("Fini"); };

SequenceBuilder<DataContext>
    .Please()
        .Do(setToTen).Then()
        .Do(increment).ThenIf(AboveTwenty, loopWhileNotMet: true)
        .Do(printDone).Then()
    .ThankYou()
    .Run(this);
 ```
