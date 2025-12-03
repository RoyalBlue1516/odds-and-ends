using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SnakeSkill : MonoBehaviour,ISkillHandler
{
    float _startRadius=0f;
    float _endRadius=4f;   
    float _currentRadius;
    bool _isSkillActive;
    bool isSkill;
    const float _skillDistance=6f;
    bool _skillSuccess;
    IEnumerator _skillRoutine;
    GameObject _clone;
    [SerializeField] GameObject _rangePrefab;
    [SerializeField] LayerMask _enemyLayer;
    [SerializeField] LayerMask _groundLayer;
    Tilemap _groundTile;
    PlayerController controller;

    void SkillFalse()
    {
        isSkill=false;
    }
    void SubscribeEvent()
    {
        controller.input.OnSkillPressed += HandlerSkill;
        controller.input.OnSkillCanceled += HandlerCancelSkill;
    }

    void OnDisable()
    {
        controller.input.OnSkillPressed -= HandlerSkill;
        controller.input.OnSkillCanceled -= HandlerCancelSkill;
    }
    public void UseSkill()
    {
        if(controller.playeranimator.GetCurrentStateInfo().IsName("Snake_Skill")) return;
        isSkill=true;
        _skillRoutine=SkillRoutine();
        StartCoroutine(_skillRoutine);
    }
    public void CancelSkill()
    {
        _isSkillActive=false;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, _currentRadius,_enemyLayer);
        if (hit != null)
        {
            Vector2 dir = hit.transform.position - transform.position;
            dir.Normalize();
            Vector2 targetPos=new Vector2(transform.position.x+_skillDistance*dir.x,transform.position.y+_skillDistance*dir.y);

            Vector3Int cellPos=_groundTile.WorldToCell(targetPos);

            transform.localScale= dir.x>0? new Vector2(1f,1f) : new Vector2(-1f,1f);

            if (!_groundTile.HasTile(cellPos))
            {
                controller.playeranimator.animator.SetTrigger("SkillFire");
                transform.Translate(new Vector2(_skillDistance*dir.x,_skillDistance*dir.y));
            }
            else
            {
                RaycastHit2D distToWall=Physics2D.Raycast(targetPos,-dir,_skillDistance,_groundLayer);
                float correctionDist= dir.x>=0? 1f: -1f;
                float dist=_skillDistance-distToWall.distance-correctionDist;
                controller.playeranimator.animator.SetTrigger("SkillFire");
                transform.Translate(dist*dir);
            }

            _skillSuccess=true;
            Debug.Log("Hirs");
        }
        else
        {
            _skillSuccess=false;
        }
        controller.playeranimator.animator.SetBool("Skill",false);
        Destroy(_clone);
        _currentRadius=_startRadius;
        controller.movement.Rb.gravityScale = controller.movement.OriginalGravity;
        if(!_skillSuccess)
            SkillFalse();
    }
    IEnumerator SkillRoutine()
    {
        controller.movement.Rb.gravityScale = 0f;
        controller.movement.Rb.linearVelocity = Vector2.zero;
        _isSkillActive = true;
        controller.playeranimator.animator.SetBool("Skill",true);
        SpawnPrefab();
        float t = 0f;
        while (t <= 1f&&_isSkillActive)
        {
            t += Time.deltaTime;
            _currentRadius = Mathf.Lerp(_startRadius, _endRadius, t);
            yield return null;
        }
        _currentRadius = _isSkillActive ? _endRadius : _startRadius;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position,_currentRadius);
    }
    void SpawnPrefab()
    {
        _clone = Instantiate(_rangePrefab, transform.position,quaternion.identity);
    }
    public bool CheckSkill()
    {
        return isSkill;
    }
    public void HandlerSkill()
    {
        UseSkill();
    }
    public void HandlerCancelSkill()
    {
        CancelSkill();
    }
    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SubscribeEvent();
        _groundTile=GameObject.FindWithTag("Ground").GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
