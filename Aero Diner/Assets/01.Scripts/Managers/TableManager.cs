using System.Collections.Generic;
using UnityEngine;

public class TableManager : Singleton<TableManager>
{
     [Header("테이블 설정")]
     [SerializeField] private Table[] tables;
     [SerializeField] private bool[] seatOccupied;
     
     [Header("Debug")]
     [SerializeField] private bool showDebugInfo = true;
     
     #region Unity Events

     protected override void Awake()
     {
          base.Awake();
          InitializeTables();
     }

     #endregion

     private void InitializeTables()
     {
          if (tables == null || tables.Length == 0)
          {
               Debug.LogError("[TableManager] Table 배열 설정 안 됨 !!!");
               return;
          }

          seatOccupied = new bool[tables.Length];
          
          for (int i = 0; i < seatOccupied.Length; i++)
          {
               seatOccupied[i] = false;
               
               if (tables[i] != null)
               {
                    tables[i].SetSeatIndex(i);
               }
          }
          
     }

     public bool AssignSeatToCustomer(CustomerController customer)
     {
          for (int i = 0; i < tables.Length; i++)
          {
               if (!seatOccupied[i])
               {
                    seatOccupied[i] = true;
                    customer.SetAssignedTable(tables[i]);
                    tables[i].AssignCustomer(customer);
                    
                    return true;
               }
          }

          return false;
     }

     public void ReleaseSeat(CustomerController customer)
     {
          Table customerTable = customer.GetAssignedTable();
          
          for (int i = 0; i < tables.Length; i++)
          {
               if (tables[i] == customerTable)
               {
                    seatOccupied[i] = false;
                    tables[i].ReleaseCustomer();
                    customer.SetAssignedTable(null);
                    break;
               }
          }
     }
     
     public bool IsTableOccupied(Table table)
     {
          for (int i = 0; i < tables.Length; i++)
          {
               if (tables[i] == table)
               {
                    return seatOccupied[i];
               }
          }
          return false;
     }
     
     public int GetAvailableSeatCount()
     {
          if (seatOccupied == null) return 0;

          int availableCount = 0;
          foreach (bool occupied in seatOccupied)
          {
               if (!occupied) availableCount++;
          }
          return availableCount;
     }

     #region public getters
     
     public bool HasAvailableSeat() => GetAvailableSeatCount() > 0;
     public int TotalSeatCount => tables.Length;
     
     #endregion
}
