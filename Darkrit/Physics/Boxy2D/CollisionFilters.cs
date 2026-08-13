namespace Darkrit.Physics.Boxy2D;

public delegate CollisionResponseFunction CollisionFilterFunction<T>(ref Body<T> self, ref Body<T> other);

public static class CollisionFilters<T>
{
    public static CollisionFilterFunction<T> Response(CollisionResponseFunction action) => (ref Body<T> _, ref Body<T> _) => action;

    public static CollisionFilterFunction<T> Stop => Response(CollisionResponses.Stop);
    public static CollisionFilterFunction<T> Slide => Response(CollisionResponses.Slide);
    public static CollisionFilterFunction<T> Push => Response(CollisionResponses.Push);
    public static CollisionFilterFunction<T> Cross => Response(CollisionResponses.Cross);
}
