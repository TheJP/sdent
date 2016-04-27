using UnityEngine;
using System.Collections;

public abstract class RtsUnit : RtsEntity
{

    private Vector3? target = null;

    public virtual float Speed
    {
        get { return 1f; }
    }

    public override void DoRightClickAction(Vector3 position)
    {
        //Only the local (authorized) player is allowed to move units.
        if (hasAuthority) { target = position; }
    }

    protected override void Update()
    {
        base.Update();
        if (hasAuthority && target.HasValue)
        {
            var direction = target.Value - transform.position;
            //The y is kept the same. (So that units don't go flying)
            direction.Scale(new Vector3(1, 0, 1));
            if (direction.magnitude < Time.deltaTime * Speed)
            {
                transform.position = new Vector3(target.Value.x, transform.position.y, target.Value.z);
                target = null;
            }
            else { transform.position += Time.deltaTime * Speed * direction.normalized; }
            transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        }
    }
}
