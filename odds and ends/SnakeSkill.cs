using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class SnakeSkill : MonoBehaviour,ISkillHandler
{
    float _startRadius=0f;
    float _endRadius=4f;   
    float _currentRadius;
    bool _isSkillActive;
    bool isSkill;
    IEnumerator _skillRoutine;
    GameObject _clone;
    [SerializeField] GameObject _rangePrefab;
    [SerializeField] LayerMask _enemyLayer;
    PlayerController controller;
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
        isSkill=true;
        _skillRoutine=SkillRoutine();
        StartCoroutine(_skillRoutine);
    }
    public void CancelSkill()
    {
        controller.playeranimator.animator.SetBool("Skill",false);
        isSkill=false;
        _isSkillActive=false;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, _currentRadius,_enemyLayer);
         if (hit != null)
            {
                Debug.Log("Hirs");
            }
        Destroy(_clone);
        _currentRadius=_startRadius;
    }
    IEnumerator SkillRoutine()
    {
        controller.movement.Rb.linearVelocityX=0f;
        _isSkillActive = true;
        controller.playeranimator.animator.SetBool("Skill",true);
        SpawnPrefab();
        // 1) 반경 확장
        yield return StartCoroutine(ExpandRadius());

        _isSkillActive = false;
    }

    IEnumerator ExpandRadius()
    {
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
    }

    // Update is called once per frame
    void Update()
    {

    }
}
