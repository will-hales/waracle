namespace Waracle.Api.Models;

public record CreateBookingViewModel() {
    public int RoomId { get; set; }
    
    private DateTime _fromDate;
    public DateTime FromDate 
    { 
        get => _fromDate.Date; 
        set => _fromDate = value.Date; 
    }

    private DateTime _toDate;
    public DateTime ToDate 
    { 
        get => _toDate.Date; 
        set => _toDate = value.Date; 
    }
    
    public int NumberOfGuests { get; set; }

    public bool IsValid() {
        return FromDate > DateTime.Now.Date && 
               ToDate > DateTime.Now.Date && 
               FromDate < ToDate;
    }   
}
