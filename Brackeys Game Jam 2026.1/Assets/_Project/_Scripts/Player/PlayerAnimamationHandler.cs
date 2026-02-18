using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class PlayerAnimamationHandler : MonoBehaviour {
    [SerializeField] private Animator animator;

    #region ===========================- Animation Hashs -===========================

    public static readonly int Idle = Animator.StringToHash("Idling");
    public static readonly int Walking = Animator.StringToHash("Walking");
    public static readonly int Sprinting = Animator.StringToHash("Sprinting");
    public static readonly int Running = Animator.StringToHash("Running");
    public static readonly int LandingSoft = Animator.StringToHash("LandingSoft");
    public static readonly int LandingHard = Animator.StringToHash("LandingHard");
    public static readonly int Falling = Animator.StringToHash("Falling");
    public static readonly int Jumping = Animator.StringToHash("Jumping");
    public static readonly int Jump = Animator.StringToHash("Jump");
    public static readonly int AirHanging = Animator.StringToHash("AirHanging"); 
    public static readonly int WallSliding = Animator.StringToHash("WallSliding");
    public static readonly int Turning = Animator.StringToHash("Turning");
    public static readonly int Dodging = Animator.StringToHash("Dodging");

    #endregion

    public event UnityAction AnimationCompleted_Action;

    private int _currentAnimation;

    public int CurrentAnimation { get { return _currentAnimation; } }

    public void Complete() {
        AnimationCompleted_Action?.Invoke();
    }

    public void Play(int hash, bool isOverrideCurrentAnimation) {
        //if (isOverrideCurrentAnimation || hash != _currentAnimation) {
        //    _currentAnimation = hash;
        //    animator.CrossFade(hash, 0, 0);
        //}
    }
}
