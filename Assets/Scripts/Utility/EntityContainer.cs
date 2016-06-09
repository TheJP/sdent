using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Container, which stores instances of subtypes of <see cref="RtsEntity"/>.
/// </summary>
public class EntityContainer : IEnumerable<RtsEntity>, ICollection<RtsEntity>
{
    private readonly Dictionary<Type, List<RtsEntity>> entities = new Dictionary<Type, List<RtsEntity>>();

    public int Count
    {
        get { return entities.Values.Sum(list => list.Count); }
    }

    public bool IsReadOnly
    {
        get { return false; }
    }

    public IEnumerable<Type> Types
    {
        get { return entities.Keys; }
    }

    /// <summary>Add the given entity to the container.</summary>
    /// <param name="entity"></param>
    public void Add(RtsEntity entity)
    {
        var type = entity.GetType();
        if (!entities.ContainsKey(type)) { entities.Add(type, new List<RtsEntity>()); }
        entities[type].Add(entity);
    }

    /// <summary>Remove the given entity from the container.</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool Remove(RtsEntity entity)
    {
        var type = entity.GetType();
        if (entities.ContainsKey(type)) { return entities[type].Remove(entity); }
        return false;
    }

    /// <summary>Returns all entities which have the given type.</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> Get<T>() where T : RtsEntity
    {
        if (entities.ContainsKey(typeof(T))) { return (IEnumerable<T>)entities[typeof(T)].AsReadOnly(); }
        else { return Enumerable.Empty<T>(); }
    }

    /// <summary>Returns all entities which have the given type.</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<RtsEntity> Get(Type entityType)
    {
        if (entities.ContainsKey(entityType)) { return entities[entityType].AsReadOnly(); }
        else { return Enumerable.Empty<RtsEntity>(); }
    }

    public IEnumerator<RtsEntity> GetEnumerator() { return entities.SelectMany(type => type.Value).GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return entities.SelectMany(type => type.Value).GetEnumerator(); }

    public void Clear() { entities.Clear(); }

    public bool ContainsType<T>() where T : RtsEntity
    {
        return entities.ContainsKey(typeof(T)) && entities[typeof(T)].Count > 0;
    }

    public bool ContainsType(Type entityType)
    {
        return entities.ContainsKey(entityType) && entities[entityType].Count > 0;
    }

    public bool Contains(RtsEntity item)
    {
        var type = item.GetType();
        return entities.ContainsKey(type) && entities[type].Contains(item);
    }

    public void CopyTo(RtsEntity[] array, int arrayIndex)
    {
        foreach (var entity in this) { array[arrayIndex++] = entity; }
    }
}
