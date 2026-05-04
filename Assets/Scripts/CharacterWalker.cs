using System.Collections;
using Events;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class CharacterWalker : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float walkSpeed = 120f;
    [SerializeField] private float swingAngle = 20f;
    [SerializeField] private float cycleDuration = 0.5f;

    // Ширина персонажа + небольшой запас, чтобы он полностью скрылся
    private const float CharacterWidth = 200f;

    private VisualElement container;
    private VisualElement leftHand, rightHand, leftLeg, rightLeg;

    [Inject] private EventBus _eventBus;
    
    private void Start()
    {
        var root = uiDocument.rootVisualElement;
        container = root.Q<VisualElement>("character-container");
        leftHand = root.Q<VisualElement>("left-hand");
        rightHand = root.Q<VisualElement>("right-hand");
        leftLeg = root.Q<VisualElement>("left-leg");
        rightLeg = root.Q<VisualElement>("right-leg");

        // Начальная позиция — левее видимой области
        container.style.translate = new Translate(-CharacterWidth, 0);

        StartCoroutine(WalkInAndStop());
    }

    private IEnumerator WalkInAndStop()
    {
        float startX = -CharacterWidth;
        float endX = 0f;
        float distance = endX - startX;
        float duration = distance / walkSpeed;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float currentX = Mathf.Lerp(startX, endX, t);
            container.style.translate = new Translate(currentX, 0);

            // Анимация шага
            float phase = (timer % cycleDuration) / cycleDuration * 2f * Mathf.PI;
            leftLeg.style.rotate = new Rotate(Angle.Degrees(Mathf.Sin(phase) * swingAngle));
            rightLeg.style.rotate = new Rotate(Angle.Degrees(-Mathf.Sin(phase) * swingAngle));
            leftHand.style.rotate = new Rotate(Angle.Degrees(-Mathf.Sin(phase) * swingAngle));
            rightHand.style.rotate = new Rotate(Angle.Degrees(Mathf.Sin(phase) * swingAngle));

            yield return null;
        }

        // Остановка в финальной позиции (translate = (0,0) — теперь позиция из CSS)
        container.style.translate = new Translate(0, 0);
        leftLeg.style.rotate = Rotate.None();
        rightLeg.style.rotate = Rotate.None();
        leftHand.style.rotate = Rotate.None();
        rightHand.style.rotate = Rotate.None();
        
        _eventBus.BubblePopup.RaiseEvent();
        _eventBus.BubbleShow.RaiseEvent("Привет!\nДавай ка попробуем сначала найти рифму для нескольких слов!");
    }
}