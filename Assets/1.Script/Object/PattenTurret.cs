using System;
using System.Collections;
using UnityEngine;

public class PattenTurret : MonoBehaviour //메인 클래스
{
   public  float         isPattenInterval;
   private float         isPattenIntervalCount;
   
   public  TurretArray[] turretArray; //열에 해당되는 이름
   private int           currentPattenNum;
   
   public Coroutine      isPattenCoroutine;
   
   private void FixedUpdate()
   {
       if (isPattenCoroutine == null)
       {
           bool isAllTurretOn = true;   // 모든 터렛이 켜져있는지 체크
           foreach (TurretArray turretArray in turretArray)
           {
               foreach (Turret turret in turretArray.turrets)
               {
                   if (!turret.gameObject.activeInHierarchy || turret.isDisabled)
                   {
                       isAllTurretOn = false;
                   }
               }
           }
           
           // 모든 터렛이 켜져있으면, 코루틴 실행
           if (isAllTurretOn)
               isPattenCoroutine = StartCoroutine(TurretPatten());
       }
   }

   private IEnumerator TurretPatten()
   {
       while (true)
       {
           while (true)
           {
               // 공격중인지 체크
               bool isPattenAttackStart = true;
               foreach (Turret turret in turretArray[currentPattenNum].turrets)
               {
                   // 각 Turret의 isAttackCoroutine 변수에 접근하여 상태 확인
                   if (turret.isAttackCoroutine)
                   {
                        // 1개라도 공격중이면, false
                       isPattenAttackStart = false;
                   }
               }
               
               // 공격중이 아니면, 공격
               if (isPattenAttackStart)
               {
                   // // 성능실험실!! 마지막 카운트 전에 넘기기
                   // if (EventController.instance.isPerformanceLab && EventController.instance.tutorialShootCount == EventController.instance.tutorialShootMaxCount - 1)
                   // {
                   //     currentPattenNum++;
                   //     break;
                   // }
                   
                   // 공격
                   foreach (Turret turret in turretArray[currentPattenNum].turrets)
                   {
                        turret.isAttackCoroutine = true;
                        StartCoroutine(turret.StraightTurretAttack());
                   }
                   
                   currentPattenNum++;
                   break;
               }
               yield return new WaitForFixedUpdate();
           }
           
           // 값을 넘어가면, 초기화
           if (currentPattenNum >= turretArray.Length)
               currentPattenNum = 0;
           
           // 인터벌 기다리기
           while (true)
           {
               isPattenIntervalCount += Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;

               if (isPattenIntervalCount > isPattenInterval)
               {
                   isPattenIntervalCount = 0f;
                   break;
               }
               yield return new WaitForFixedUpdate();
           }
           
           //EventController.instance.tutorialShootCount++;
           
           yield return new WaitForFixedUpdate();
       }
   }

   private void OnDisable()
   {
       isPattenCoroutine = null;
       currentPattenNum  = 0;
   }

   public void StopPattenCoroutine()
   {
       StopAllCoroutines();
       
       isPattenCoroutine = null;
       currentPattenNum  = 0;
       isPattenIntervalCount = 0;
       
       for (int i = 0; i < turretArray.Length; i++)
       {
           foreach (Turret turret in turretArray[i].turrets)
           {
               turret.isAttackCoroutine = false;
               StopCoroutine(turret.StraightTurretAttack());
           }
       }
   }
}

[Serializable] //반드시 필요
public class TurretArray //행에 해당되는 이름
{
    public Turret[] turrets;
}