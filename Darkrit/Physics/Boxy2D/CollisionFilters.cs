using Microsoft.Xna.Framework;

namespace Darkrit.Physics.Boxy2D;

//public delegate CollisionResponseFunction CollisionFilterFunction<T, TContext>(ref Body<T> self, ref Body<T> other, TContext context);

public delegate CollisionResponseFunction CollisionFilterFunction<T, TContext>(ref Body<T> self, ref Body<T> other, ref Vector2 motion, CollisionInfo collisionInfo, TContext context);
public static class CollisionFilters<T, TContext>
{
    public static CollisionFilterFunction<T, TContext> Response(CollisionResponseFunction action) => (ref Body<T> _, ref Body<T> _, ref Vector2 _, CollisionInfo _, TContext _) => action;

    public static CollisionFilterFunction<T, TContext> Stop => Response(CollisionResponses.Stop);
    public static CollisionFilterFunction<T, TContext> Slide => Response(CollisionResponses.Slide);
    public static CollisionFilterFunction<T, TContext> Push => Response(CollisionResponses.Push);
    public static CollisionFilterFunction<T, TContext> Cross => Response(CollisionResponses.Cross);
}
