using System;
using UnityEngine;
using Sequencer;
public class Cutscene_Test : MonoBehaviour
{

    [SerializeField] private Transform _stuffToMove;


    public class DataContext
    {
        public int i;
    }

    void Start()
    {

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


        SequenceBuilderNoContext
          .Please()
              .Do((ctx) =>
              {
                  _stuffToMove.transform.Translate(_stuffToMove.forward * Time.deltaTime * 5);
              })
          .ThenIf((ctx) => Vector3.Distance(Vector3.zero, _stuffToMove.transform.position) > 10, loopWhileNotMet: true )
              .DoIfDebug((ctx) =>
              {
                  Debug.Log("Done");
              })
          .Then()
          .ThankYou()
          .Run(this);

    }
}
