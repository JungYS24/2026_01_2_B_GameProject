using UnityEngine;
using DG.Tweening;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public int moneyAmount = 10;

    private bool isPicked = false;

    void Start()
    {
        Vector3 randomPosition = transform.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));

        transform.DOJump(randomPosition, 1.2f, 1, 0.4f).SetLink(gameObject);      //SetLink 오브젝트가 없어질때 같이 사라짐
        transform.DORotate(new Vector3(0f, 360f, 0f), 0.4f, RotateMode.FastBeyond360).SetLink(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPicked) return; // 이미 줍기 처리된 경우 무시

        if (other.CompareTag("Player"))
        {
            isPicked = true; // 줍기 처리 상태로 변경
            MoneyUI moneyUI = Object.FindFirstObjectByType<MoneyUI>();
            if (moneyUI != null)
            {
                moneyUI.GetMoney(moneyAmount, transform.position);
            }
            transform.DOKill();
            Destroy(gameObject);
        }
    }
}
