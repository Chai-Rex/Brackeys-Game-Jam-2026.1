using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class PlayerAnimamationHandler : MonoBehaviour {
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
    public static readonly string LandingSoft = "landing";
    public static readonly string LandingHard = "landing";
    public static readonly string Falling = "falling";
    public static readonly string Jumping = "jump2";
    public static readonly string Jump = "jump1";
    public static readonly string AirHanging = "falling"; 
    public static readonly string WallSliding = "falling";
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
