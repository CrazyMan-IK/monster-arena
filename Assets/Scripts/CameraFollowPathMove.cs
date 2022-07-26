using System.Collections;
using Cinemachine;
using UnityEngine;

public class CameraFollowPathMove : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _cinemachineCamera;
    [SerializeField] private CinemachineVirtualCamera _huggyCamera;
    [SerializeField] private float _speed;
    [SerializeField] private bool _isMove;
    [SerializeField] private GameObject _soldiers;
    [SerializeField] private Animator _huggyAnimator;

    private CinemachineTrackedDolly _cinemachineCameraDolly;

    private void Start()
    {
        _cinemachineCameraDolly = _cinemachineCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        _cinemachineCameraDolly.m_PathPosition = 0;

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        while (_cinemachineCameraDolly.m_PathPosition < 0.3f)
        {
            Move();
            yield return null;
        }
        
        _soldiers.SetActive(true);

        while (_cinemachineCameraDolly.m_PathPosition < 0.5f)
        {
            Move();
            yield return null;
        }
        
        yield return new WaitForSeconds(1.2f);

        _huggyAnimator.SetTrigger("Win");
        yield return new WaitForSeconds(0.3f);
        _huggyAnimator.SetTrigger("Win");

        _huggyCamera.gameObject.SetActive(true);

        yield return new WaitForSeconds(4f);

        _huggyCamera.gameObject.SetActive(false);
        _cinemachineCamera.Priority = 0;

    }

    private void Move()
    {
        _cinemachineCameraDolly.m_PathPosition += _speed * Time.deltaTime;
    }

    private void Update()
    {
        
        
    }
}
