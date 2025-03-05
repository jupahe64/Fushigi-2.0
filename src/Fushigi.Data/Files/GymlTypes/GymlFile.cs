using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

// ReSharper disable once InconsistentNaming
public struct INHERITED<TValue>
{
    internal bool IsPresent;
    internal TValue Value;
}

/// <summary>
/// Works around an issue where you can't have ref parameters in lambdas without writing out the full type
/// </summary>
/// <typeparam name="T"></typeparam>
public ref struct ByRef<T>(ref T value)
{
    public ref T _ = ref value;
}

public abstract class GymlFile<T> : SerializableBymlObject<T>
    where T : GymlFile<T>, IGymlType, new()
{
    protected abstract T This { get; }
    public delegate ref INHERITED<TValue> ValueAccessor<TValue>(T file);
    public delegate ref INHERITED<TValueOut> SubValueAccessor<TValueIn, TValueOut>(ByRef<TValueIn> target);

    #region Inherited Property get/set
    public TValue Get<TValue>(ValueAccessor<TValue> accessor)
    {
        foreach (var element in new IterableHierarchy(this))
        {
            ref var inheritedValue = ref accessor(element.This);
            if (inheritedValue.IsPresent)
                return inheritedValue.Value;
        }
        return default!;
    }
    
    public TValue2 Get<TValue1, TValue2>(
        ValueAccessor<TValue1> accessor, 
        SubValueAccessor<TValue1, TValue2> subAccessor)
    {
        foreach (var element in new IterableHierarchy(this))
        {
            ref var inheritedValue1 = ref accessor(element.This);
            if (!inheritedValue1.IsPresent) continue;
            
            ref var inheritedValue2 = ref subAccessor(new ByRef<TValue1>(ref inheritedValue1.Value));
            if (!inheritedValue2.IsPresent) continue;
            
            return inheritedValue2.Value;
        }
        return default!;
    }
    
    public TValue3 Get<TValue1, TValue2, TValue3>(
        ValueAccessor<TValue1> accessor, 
        SubValueAccessor<TValue1, TValue2> subAccessor1,
        SubValueAccessor<TValue2, TValue3> subAccessor2)
    {
        foreach (var element in new IterableHierarchy(this))
        {
            ref var inheritedValue1 = ref accessor(element.This);
            if (!inheritedValue1.IsPresent) continue;
            
            ref var inheritedValue2 = ref subAccessor1(new(ref inheritedValue1.Value));
            if (!inheritedValue2.IsPresent) continue;
            
            ref var inheritedValue3 = ref subAccessor2(new(ref inheritedValue2.Value));
            if (!inheritedValue3.IsPresent) continue;
            
            return inheritedValue3.Value;
        }
        return default!;
    }
    
    public void Set<TValue>(ValueAccessor<TValue> accessor, TValue value)
    {
        ref var inheritedValue = ref accessor(This);
        inheritedValue.Value = value;
        inheritedValue.IsPresent = true;
    }
    
    public void Set<TValue1, TValue2>(ValueAccessor<TValue1> accessor, 
        SubValueAccessor<TValue1, TValue2> subAccessor, 
        TValue2 value)
    {
        ref var inheritedValue1 = ref accessor(This);
        inheritedValue1.IsPresent = true;
        ref var inheritedValue2 = ref subAccessor(new(ref inheritedValue1.Value));
        inheritedValue2.IsPresent = true;
        
        inheritedValue2.Value = value;
    }
    
    public void Set<TValue1, TValue2, TValue3>(ValueAccessor<TValue1> accessor, 
        SubValueAccessor<TValue1, TValue2> subAccessor1, 
        SubValueAccessor<TValue2, TValue3> subAccessor2, 
        TValue3 value)
    {
        ref var inheritedValue1 = ref accessor(This);
        inheritedValue1.IsPresent = true;
        ref var inheritedValue2 = ref subAccessor1(new(ref inheritedValue1.Value));
        inheritedValue2.IsPresent = true;
        ref var inheritedValue3 = ref subAccessor2(new(ref inheritedValue2.Value));
        inheritedValue3.IsPresent = true;
        
        inheritedValue3.Value = value;
        inheritedValue3.Value = value;
    }
    #endregion
    
    public T? Parent => _parent;
    
    private T? _parent;
    private GymlRef<T>? _parentGymlRef;

    protected override void Serialization<TContext>(TContext ctx)
    {
        ctx.Set(GYML_REF<T>(), ref _parentGymlRef!, "$parent", optional: true);
    }
    
    internal GymlRef<T>? ParentGymlRef => _parentGymlRef;

    internal void SetParent(T parent)
    {
        _parent = parent;
    }

    private readonly struct IterableHierarchy(GymlFile<T> file)
    {
        public Enumerator GetEnumerator() => new(file);
        
        // adapted from https://github.com/dotnet/runtime/
        // System.Collections.Immutable/src/System/Collections/Immutable/ImmutableStack_1.Enumerator.cs
        public struct Enumerator(GymlFile<T> file)
        {
            private readonly GymlFile<T> _originalHierarchy = file;

            private GymlFile<T>? _remainingHierarchy = null;

            /// <summary>
            /// Gets the current element.
            /// </summary>
            public GymlFile<T> Current => _remainingHierarchy ?? throw new InvalidOperationException();

            /// <summary>
            /// Moves to the first or next element.
            /// </summary>
            /// <returns>A value indicating whether there are any more elements.</returns>
            public bool MoveNext()
            {
                if (_remainingHierarchy == null)
                {
                    // initial move
                    _remainingHierarchy = _originalHierarchy;
                }
                else if (_remainingHierarchy != null)
                {
                    _remainingHierarchy = _remainingHierarchy.Parent;
                }

                return _remainingHierarchy != null;
            }
        }
    }
}