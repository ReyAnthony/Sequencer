using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using JetBrains.Annotations;


public interface ISequence<TContext, Runner>
{
    void Run(Runner runner);
}

public struct SequenceData<TContext>
{
    private readonly Action<TContext> _doEvent;
    private readonly Func<TContext, bool> _condition;
    private readonly bool _loop;

    public SequenceData(Action<TContext> doEvent, Func<TContext, bool> condition, bool loop = false)
    {
        _condition = condition;
        _loop = loop;
        _doEvent = doEvent;
    }

    public Func<TContext, bool> Condition => _condition;
    public Action<TContext> DoEvent => _doEvent;
    public bool Loop => _loop;
}

public class SequenceBuilder<TContext> where TContext : class, new()
{
    private class CoroutineSequence<Runner> : ISequence<TContext, Runner> where Runner : MonoBehaviour
    {
        private readonly List<SequenceData<TContext>> _sequenceData;
        private TContext _context;
        private bool _firstRun;

        public CoroutineSequence(List<SequenceData<TContext>> sequenceData)
        {
            _sequenceData = sequenceData;
        }

        public void Run(Runner runner)
        {
            runner.StartCoroutine(RunCoroutine());
        }

        [UsedImplicitly]
        private IEnumerator RunCoroutine()
        {
            _context = new TContext();

            foreach (var sd in _sequenceData)
            {
                _firstRun = true;

                do
                {
                    if (_firstRun || sd.Loop)
                    {
                        sd.DoEvent?.Invoke(_context);
                        _firstRun = false;
                    }

                    yield return new WaitForEndOfFrame();

                } while ((!sd.Condition?.Invoke(_context)) ?? false);
            }
        }
    }

    private List<SequenceData<TContext>> _doEvents;

    protected SequenceBuilder()
    {
        _doEvents = new List<SequenceData<TContext>>();
    }

    public static SequenceBuilder<TContext> Please() => new SequenceBuilder<TContext>();

    public IDoBuilderAllTraits<TContext> Do(Action<TContext> doIt) => new DoBuilder<TContext>(this, _doEvents, doIt);
    public IDoBuilderThenTrait<TContext> DoIfDebug(Action<TContext> doIt)
    {
#if UNITY_EDITOR || DEBUG
        return new DoBuilder<TContext>(this, _doEvents, doIt);
#else
        return new DoBuilder<TContext>(this, _doEvents, null);
#endif
    }
    public ISequence<TContext, MonoBehaviour> ThankYou() => new CoroutineSequence<MonoBehaviour>(_doEvents);
}

public class DummyDataContext { }
public class SequenceBuilderNoContext : SequenceBuilder<DummyDataContext> { }

public interface IDoBuilderThenTrait<TContext> where TContext : class, new()
{
    public SequenceBuilder<TContext> Then();
}

public interface IDoBuilderThenIfTrait<TContext> where TContext : class, new()
{
    public SequenceBuilder<TContext> ThenIf(Func<TContext, bool> condition, bool loopWhileNotMet);
}

public interface IDoBuilderAllTraits<TContext> : IDoBuilderThenIfTrait<TContext>, IDoBuilderThenTrait<TContext> where TContext : class, new()
{ 
}

public class DoBuilder<TContext> : IDoBuilderAllTraits<TContext> where TContext : class, new()
{
    private SequenceBuilder<TContext> _sequenceBuilder;
    private List<SequenceData<TContext>> _doEvents;
    private Action<TContext> _doIt;

    public DoBuilder(SequenceBuilder<TContext> sequenceBuilder, List<SequenceData<TContext>> doEvents, Action<TContext> doIt)
    {
        this._sequenceBuilder = sequenceBuilder;
        this._doEvents = doEvents;
        this._doIt = doIt;
    }

    public SequenceBuilder<TContext> Then()
    {
        if (_doIt == null) return _sequenceBuilder;

        _doEvents.Add(new SequenceData<TContext>(_doIt, null));
        return _sequenceBuilder;
    }

    public SequenceBuilder<TContext> ThenIf(Func<TContext, bool> condition, bool loopWhileNotMet)
    {
        if (_doIt == null) return _sequenceBuilder;

        _doEvents.Add(new SequenceData<TContext>(_doIt, condition, loopWhileNotMet));
        return _sequenceBuilder;
    }
}
