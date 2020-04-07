using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web;

namespace Eticaret.Services.Senders
{
    public interface IMessageService
    {
        MessageState messageState { get; }

        Task SendAsync(IdentityMessage message, params string[] concacts);//asenkron gönder metodu
        void Send(IdentityMessage message, params string[] concacts);//senkron gönder metodu
    }
}