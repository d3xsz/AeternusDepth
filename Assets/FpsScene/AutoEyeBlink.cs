// FinalEyeBlink.cs - SON HAL
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FinalEyeBlink : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(EyeAnimation());
    }

    IEnumerator EyeAnimation()
    {
        // 1. CANVAS
        GameObject canvas = new GameObject("Canvas");
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        // 2. PUSLU ARKA PLAN (Başta kapalı)
        GameObject blurBG = CreateFullScreen(canvas.transform, new Color(0, 0, 0, 0.3f));
        blurBG.SetActive(false);

        // 3. ÜST VE ALT BARLAR
        GameObject topBar = CreateBar(canvas.transform, true, 540f);
        GameObject bottomBar = CreateBar(canvas.transform, false, 540f);

        RectTransform topRect = topBar.GetComponent<RectTransform>();
        RectTransform bottomRect = bottomBar.GetComponent<RectTransform>();

        yield return new WaitForSeconds(0.5f);

        // ADIM 1: BARLAR AÇILIYOR (PUSLU AKTİF)
        blurBG.SetActive(true);

        for (float t = 0; t < 1.5f; t += Time.deltaTime)
        {
            float h = Mathf.Lerp(540f, 200f, t / 1.5f);
            topRect.sizeDelta = new Vector2(1920, h);
            bottomRect.sizeDelta = new Vector2(1920, h);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // ADIM 2: BARLAR KAPANIYOR (PUSLU HALA AKTİF)
        for (float t = 0; t < 1.5f; t += Time.deltaTime)
        {
            float h = Mathf.Lerp(200f, 540f, t / 1.5f);
            topRect.sizeDelta = new Vector2(1920, h);
            bottomRect.sizeDelta = new Vector2(1920, h);
            yield return null;
        }

        blurBG.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        // ADIM 3: BARLAR TAMAMEN AÇILIYOR (PUSLU YOK)
        for (float t = 0; t < 1.5f; t += Time.deltaTime)
        {
            float h = Mathf.Lerp(540f, 0f, t / 1.5f);
            topRect.sizeDelta = new Vector2(1920, h);
            bottomRect.sizeDelta = new Vector2(1920, h);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // ADIM 4: GÖZ KRIPMA (PUSLU ANİMASYONLU)
        for (int i = 0; i < 2; i++)
        {
            // KAPAN + PUSLU AÇ
            blurBG.SetActive(true);
            for (float t = 0; t < 0.15f; t += Time.deltaTime)
            {
                float h = Mathf.Lerp(0, 490, t / 0.15f);
                topRect.sizeDelta = new Vector2(1920, h);
                bottomRect.sizeDelta = new Vector2(1920, h);
                yield return null;
            }

            // AÇ + PUSLU KAPAT
            blurBG.SetActive(false);
            for (float t = 0; t < 0.15f; t += Time.deltaTime)
            {
                float h = Mathf.Lerp(490, 0, t / 0.15f);
                topRect.sizeDelta = new Vector2(1920, h);
                bottomRect.sizeDelta = new Vector2(1920, h);
                yield return null;
            }

            if (i == 0) yield return new WaitForSeconds(0.1f);
        }

        Destroy(canvas);
    }

    GameObject CreateFullScreen(Transform parent, Color color)
    {
        GameObject obj = new GameObject("BlurBackground");
        obj.transform.SetParent(parent);

        Image img = obj.AddComponent<Image>();
        img.color = color;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return obj;
    }

    GameObject CreateBar(Transform parent, bool isTop, float height)
    {
        GameObject bar = new GameObject(isTop ? "TopBar" : "BottomBar");
        bar.transform.SetParent(parent);

        Image img = bar.AddComponent<Image>();
        img.color = Color.black;

        RectTransform rt = bar.GetComponent<RectTransform>();

        if (isTop)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
        }
        else
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
        }

        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(1920, height);

        return bar;
    }
}