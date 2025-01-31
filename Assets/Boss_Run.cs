using UnityEngine;

public class Boss_Run : StateMachineBehaviour
{
    Transform player;
    Rigidbody2D rb;

    float attackRange = 10f;
    float dashCD = 2f;

    public float speed = 4f;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = animator.GetComponent<Rigidbody2D>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Vector2 target = new Vector2(player.position.x, player.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(player.position, rb.position) <= attackRange) {
            animator.SetTrigger("shoot");
        }
        
        if (dashCD <= 0 && Vector2.Distance(player.position, rb.position) >= attackRange) {
            animator.SetTrigger("dash");
            dashCD = 2f;
        }
        dashCD -= Time.fixedDeltaTime;

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.ResetTrigger("shoot");
    }
}
