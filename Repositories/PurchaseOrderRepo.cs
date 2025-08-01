﻿namespace InventoryBeginners.Repositories
{
    public class PurchaseOrderRepo : IPurchaseOrder
    {
        private string _errors = "";

        public string GetErrors()
        {
            return _errors;
        }


        private readonly InventoryContext _context; // for connecting to efcore.
        public PurchaseOrderRepo(InventoryContext context) // will be passed by dependency injection.
        {
            _context = context;
        }
        public bool Create(PoHeader poHeader)
        {
            bool retVal = false;
            _errors = "";

            try
            {
                _context.PoHeaders.Add(poHeader);
                _context.SaveChanges();
                retVal = true;
            }
            catch (Exception ex)
            {
                _errors = "Create Failed - Sql Exception Occured , Error Info : " + ex.Message;
            }
            return retVal;
        }

        public bool Delete(PoHeader poHeader)
        {
            bool retVal = false;
            _errors = "";

            try
            {
                _context.Attach(poHeader);
                _context.Entry(poHeader).State = EntityState.Deleted;
                _context.SaveChanges();
                retVal = true;
            }
            catch (Exception ex)
            {
                _errors = "Delete Failed - Sql Exception Occured , Error Info : " + ex.Message;
            }
            return retVal;
        }

        public bool Edit(PoHeader poHeader)
        {
            bool retVal = false;
            _errors = "";

            try
            {

                List<PoDetail> poDetails = _context.PoDetails.Where(d => d.PoId == poHeader.Id).ToList();
                _context.PoDetails.RemoveRange(poDetails);
                _context.SaveChanges();

                _context.Attach(poHeader);
                _context.Entry(poHeader).State = EntityState.Modified;
                _context.PoDetails.AddRange(poHeader.PoDetails);
                _context.SaveChanges();


                retVal = true;
            }
            catch (Exception ex)
            {
                _errors = "Update Failed - Sql Exception Occured , Error Info : " + ex.Message;
            }
            return retVal;
        }




        private List<PoHeader> DoSort(List<PoHeader> items, string SortProperty, SortOrder sortOrder)
        {

            if (SortProperty.ToLower() == "ponumber")
            {
                if (sortOrder == SortOrder.Ascending)
                    items = items.OrderBy(n => n.PoNumber).ToList();
                else
                    items = items.OrderByDescending(n => n.PoNumber).ToList();
            }
            else if (SortProperty.ToLower() == "quotationno")
            {
                if (sortOrder == SortOrder.Ascending)
                    items = items.OrderBy(n => n.QuotationNo).ToList();
                else
                    items = items.OrderByDescending(n => n.QuotationNo).ToList();
            }
            else
            {
                if (sortOrder == SortOrder.Ascending)
                    items = items.OrderByDescending(d => d.Id).ToList();
                else
                    items = items.OrderBy(d => d.Id).ToList();
            }

            return items;
        }

        public PaginatedList<PoHeader> GetItems(string SortProperty, SortOrder sortOrder, string SearchText = "", int pageIndex = 1, int pageSize = 5)
        {
            List<PoHeader> items;

            if (SearchText != "" && SearchText != null)
            {
                items = _context.PoHeaders.Where(n => n.PoNumber.Contains(SearchText) || n.QuotationNo.Contains(SearchText))
                    .Include(s => s.Supplier)
                    .Include(c => c.BaseCurrency)
                    .Include(f => f.POCurrency)
                    .ToList();
            }
            else
                items = _context.PoHeaders
                   .Include(s => s.Supplier)
                   .Include(c => c.BaseCurrency)
                   .Include(f => f.POCurrency)
                   .ToList();




            items = DoSort(items, SortProperty, sortOrder);

            PaginatedList<PoHeader> retItems = new PaginatedList<PoHeader>(items, pageIndex, pageSize);

            return retItems;
        }

        public PoHeader GetItem(int Id)
        {
            PoHeader item = _context.PoHeaders.Where(i => i.Id == Id)
                     .Include(d => d.PoDetails)
                     .ThenInclude(i => i.Product)
                     .ThenInclude(u => u.Units)
                     .FirstOrDefault();


            item.PoDetails.ForEach(i => i.UnitName = i.Product.Units.Name);
            item.PoDetails.ForEach(p => p.Description = p.Product.Description);
            item.PoDetails.ForEach(p => p.Total = p.Quantity * p.PrcInBaseCur);

            return item;
        }

        public bool IsPoNumberExists(string ponumber)
        {
            int ct = _context.PoHeaders.Where(n => n.PoNumber.ToLower() == ponumber.ToLower()).Count();
            if (ct > 0)
                return true;
            else
                return false;
        }

        public bool IsPoNumberExists(string ponumber, int Id = 0)
        {
            if (Id == 0)
                return IsPoNumberExists(ponumber);

            var poHeads = _context.PoHeaders.Where(n => n.PoNumber == ponumber).Max(cd => cd.Id);
            if (poHeads == 0 || poHeads == Id)
                return false;
            else
                return true;
        }

        public bool IsQuotationNoExists(string quotationNo)
        {
            int ct = _context.PoHeaders.Where(n => n.QuotationNo.ToLower() == quotationNo.ToLower()).Count();
            if (ct > 0)
                return true;
            else
                return false;
        }
        public bool IsQuotationNoExists(string quotationNo, int Id = 0)
        {
            if (Id == 0)
                return IsQuotationNoExists(quotationNo);

            var strQuotNo = _context.PoHeaders.Where(n => n.QuotationNo == quotationNo).Max(nm => nm.Id);
            if (strQuotNo == 0 || strQuotNo == Id)
                return false;
            else
                return true;
        }

        public string GetNewPONumber()
        {

            string ponumber = "";
            var LastPoNumber = _context.PoHeaders.Max(cd => cd.PoNumber);

            if (LastPoNumber == null)
                ponumber = "PO00001";
            else
            {
                int lastdigit = 1;
                int.TryParse(LastPoNumber.Substring(2, 5).ToString(), out lastdigit);


                ponumber = "PO" + (lastdigit + 1).ToString().PadLeft(5, '0');
            }


            return ponumber;


        }
    }
}
