using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationHandler : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private SkeletonAnimation spineAnimator;

    private PlayerBlackboardHandler _blackboard;
    public void Initialize(PlayerBlackboardHandler blackboard) {
        _blackboard = blackboard;
        _blackboard.OnDirectionChanged += OnDirectionChanged;
    }

    private void OnDestroy() {
        _blackboard.OnDirectionChanged -= OnDirectionChanged;
    }

    private void OnDirectionChanged(bool isFacingRight) {
        if (isFacingRight) {
            transform.localScale = new Vector3(-1, 1, 1);
        } else {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    #region ===========================- Animation Hashs -===========================

    public static readonly string Idle = "idle";
    public static readonly string Walking = "run";
    public static readonly string Sprinting = "run";
    public static readonly string Running = "run";
    public static readonly string LandingSoft = "lannd";
    public static readonly string LandingHard = "lannd";
    public static readonly string Falling = "fall";
    public static readonly string Jumping = "jump";
    public static readonly string Jump = "jump";
    public static readonly string AirHanging = "max"; 
    public static readonly string WallSliding = "wallSliding";
    public static readonly string Turning = "idle";
    public static readonly string Dodging = "idle";

    #endregion

    public event UnityAction AnimationCompleted_Action;

    private string _currentAnimation;

    public string CurrentAnimation { get { return _currentAnimation; } }

    public void Complete() {
        AnimationCompleted_Action?.Invoke();
    }

    public void Play(string hash, bool isOverrideCurrentAnimation) {
        if (isOverrideCurrentAnimation || hash != _currentAnimation) {
            _currentAnimation = hash;
            //animator.CrossFade(hash, 0, 0);
            spineAnimator.AnimationState.SetAnimation(0, hash, true);
            //Debug.LogWarning($"Playing animation: {hash}");
        }
    }
}
