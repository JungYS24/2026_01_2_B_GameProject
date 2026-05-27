using UnityEngine;
using TMPro;
using DG.Tweening;

public class MoneyUI : MonoBehaviour
{

    public Canvas canvas; 
    public RectTransform coinIconPrefab;
    public RectTransform coinTarget;
    public TMP_Text moneyText;

    public Color flashColor = Color.yellow;
    public float flyTime = 0.5f;
    private int money = 0;

    private Color originalColor;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moneyText.text = money.ToString();
        originalColor = moneyText.color;
    }

    public void GetMoney(int amount, Vector3 worldPosition)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        RectTransform coinIcon = Instantiate(coinIconPrefab, canvas.transform);

        coinIcon.position = screenPosition;
        coinIcon.localScale = Vector3.one;

        coinIcon.DOMove(coinTarget.position, flyTime).SetEase(Ease.InBack) // UI코인 아이콘을 목표 위치로 이동
        .OnComplete(() =>
        {
            Destroy(coinIcon.gameObject); // 날아간 코인 아이콘 제거
            money += amount; // 돈 증가   
            moneyText.text = money.ToString(); //증가한 돈을 텍스트에 표시
            PlayMoneyEffect(); // UI 반응 연출 함수
        });
    }

    public void PlayMoneyEffect()
    {
        moneyText.transform.DOKill();
        moneyText.DOKill();
        moneyText.transform.localScale = Vector3.one;
        moneyText.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);

        moneyText.DOColor(flashColor, 0.1f)
        .OnComplete(() =>
        {
            moneyText.DOColor(originalColor, 0.2f);
        });
    }
}
